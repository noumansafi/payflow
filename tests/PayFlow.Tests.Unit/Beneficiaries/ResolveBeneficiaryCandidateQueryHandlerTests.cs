using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Beneficiaries.Queries.ResolveBeneficiaryCandidate;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Tests.Unit.Beneficiaries;

public sealed class ResolveBeneficiaryCandidateQueryHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IBeneficiaryRepository _beneficiaries = Substitute.For<IBeneficiaryRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private ResolveBeneficiaryCandidateQueryHandler CreateSut() =>
        new(_users, _beneficiaries, _currentUser);

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsCandidate()
    {
        var ownerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUser.UserId.Returns(ownerId);
        _users.GetByIdAsync(targetId, Arg.Any<CancellationToken>()).Returns(new User
        {
            Id = targetId,
            Email = "sara@example.com",
            PasswordHash = "hash",
            FirstName = "Sara",
            LastName = "Khan",
            CreatedAtUtc = DateTime.UtcNow
        });
        _beneficiaries.ExistsAsync(ownerId, targetId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await CreateSut().Handle(
            new ResolveBeneficiaryCandidateQuery(targetId),
            CancellationToken.None);

        result.UserId.Should().Be(targetId);
        result.FirstName.Should().Be("Sara");
        result.LastName.Should().Be("Khan");
        result.Email.Should().Be("sara@example.com");
        result.AlreadySaved.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenMissing_ThrowsNotFound()
    {
        var ownerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUser.UserId.Returns(ownerId);
        _users.GetByIdAsync(targetId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => CreateSut().Handle(
            new ResolveBeneficiaryCandidateQuery(targetId),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSelf_ThrowsValidation()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);

        var act = () => CreateSut().Handle(
            new ResolveBeneficiaryCandidateQuery(userId),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
    }
}
