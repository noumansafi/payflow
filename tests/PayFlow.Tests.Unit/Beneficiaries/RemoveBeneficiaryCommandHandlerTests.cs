using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Beneficiaries.Commands.RemoveBeneficiary;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Tests.Unit.Beneficiaries;

public sealed class RemoveBeneficiaryCommandHandlerTests
{
    private readonly IBeneficiaryRepository _beneficiaries = Substitute.For<IBeneficiaryRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RemoveBeneficiaryCommandHandler CreateSut() =>
        new(_beneficiaries, _currentUser, _unitOfWork);

    [Fact]
    public async Task Handle_WhenOwner_RemovesBeneficiary()
    {
        var ownerId = Guid.NewGuid();
        var beneficiary = new Beneficiary
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerId,
            BeneficiaryUserId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _currentUser.UserId.Returns(ownerId);
        _beneficiaries.GetByIdForOwnerAsync(beneficiary.Id, ownerId, Arg.Any<CancellationToken>())
            .Returns(beneficiary);

        await CreateSut().Handle(new RemoveBeneficiaryCommand(beneficiary.Id), CancellationToken.None);

        _beneficiaries.Received(1).Remove(beneficiary);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMissing_ThrowsNotFound()
    {
        var ownerId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _currentUser.UserId.Returns(ownerId);
        _beneficiaries.GetByIdForOwnerAsync(id, ownerId, Arg.Any<CancellationToken>())
            .Returns((Beneficiary?)null);

        var act = () => CreateSut().Handle(new RemoveBeneficiaryCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
