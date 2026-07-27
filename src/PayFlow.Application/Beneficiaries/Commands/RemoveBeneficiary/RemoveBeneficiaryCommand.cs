using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Application.Beneficiaries.Commands.RemoveBeneficiary;

public sealed record RemoveBeneficiaryCommand(Guid BeneficiaryId) : IRequest;

public sealed class RemoveBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaries,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveBeneficiaryCommand>
{
    public async Task Handle(RemoveBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid ownerUserId)
        {
            throw new UnauthorizedAppException();
        }

        var beneficiary = await beneficiaries.GetByIdForOwnerAsync(
                request.BeneficiaryId,
                ownerUserId,
                cancellationToken)
            ?? throw new NotFoundException("Beneficiary was not found.");

        beneficiaries.Remove(beneficiary);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
