using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Beneficiaries.Commands.AddBeneficiary;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Tests.Unit.Beneficiaries;

public sealed class AddBeneficiaryCommandHandlerTests
{
    private readonly IBeneficiaryRepository _beneficiaries = Substitute.For<IBeneficiaryRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private AddBeneficiaryCommandHandler CreateSut() =>
        new(_beneficiaries, _users, _currentUser, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_WhenValid_AddsBeneficiary()
    {
        var ownerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var now = new DateTime(2026, 7, 27, 10, 0, 0, DateTimeKind.Utc);
        var target = CreateUser(targetId, "sara@example.com", "Sara", "Khan");

        _clock.UtcNow.Returns(now);
        _currentUser.UserId.Returns(ownerId);
        _users.GetByIdAsync(targetId, Arg.Any<CancellationToken>()).Returns(target);
        _beneficiaries.ExistsAsync(ownerId, targetId, Arg.Any<CancellationToken>()).Returns(false);

        Beneficiary? captured = null;
        _beneficiaries.When(x => x.Add(Arg.Any<Beneficiary>()))
            .Do(ci => captured = ci.Arg<Beneficiary>());

        var result = await CreateSut().Handle(
            new AddBeneficiaryCommand(targetId, " Sara "),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.OwnerUserId.Should().Be(ownerId);
        captured.BeneficiaryUserId.Should().Be(targetId);
        captured.DisplayName.Should().Be("Sara");
        result.Email.Should().Be("sara@example.com");
        result.DisplayName.Should().Be("Sara");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSelf_ThrowsValidation()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);

        var act = () => CreateSut().Handle(
            new AddBeneficiaryCommand(userId, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTargetMissing_ThrowsNotFound()
    {
        var ownerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUser.UserId.Returns(ownerId);
        _users.GetByIdAsync(targetId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => CreateSut().Handle(
            new AddBeneficiaryCommand(targetId, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDuplicate_ThrowsConflict()
    {
        var ownerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUser.UserId.Returns(ownerId);
        _users.GetByIdAsync(targetId, Arg.Any<CancellationToken>())
            .Returns(CreateUser(targetId, "sara@example.com", "Sara", "Khan"));
        _beneficiaries.ExistsAsync(ownerId, targetId, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => CreateSut().Handle(
            new AddBeneficiaryCommand(targetId, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already*");
    }

    private static User CreateUser(Guid id, string email, string first, string last) => new()
    {
        Id = id,
        Email = email,
        PasswordHash = "hash",
        FirstName = first,
        LastName = last,
        CreatedAtUtc = DateTime.UtcNow
    };
}
