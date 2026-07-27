using MediatR;
using PayFlow.Application.Beneficiaries.Dtos;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Beneficiaries;
using PayFlow.Domain.Entities;

namespace PayFlow.Application.Beneficiaries.Commands.AddBeneficiary;

public sealed record AddBeneficiaryCommand(
    Guid BeneficiaryUserId,
    string? DisplayName) : IRequest<BeneficiaryDto>;

public sealed class AddBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IUserRepository users,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<AddBeneficiaryCommand, BeneficiaryDto>
{
    public async Task<BeneficiaryDto> Handle(
        AddBeneficiaryCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid ownerUserId)
        {
            throw new UnauthorizedAppException();
        }

        if (BeneficiaryRules.IsSelf(ownerUserId, request.BeneficiaryUserId))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.BeneficiaryUserId)] = ["Cannot add yourself as a beneficiary."]
            });
        }

        var beneficiaryUser = await users.GetByIdAsync(request.BeneficiaryUserId, cancellationToken)
            ?? throw new NotFoundException("Beneficiary user was not found.");

        if (await beneficiaries.ExistsAsync(ownerUserId, request.BeneficiaryUserId, cancellationToken))
        {
            throw new ConflictException("This user is already in your beneficiaries list.");
        }

        var now = clock.UtcNow;
        var beneficiary = new Beneficiary
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            BeneficiaryUserId = request.BeneficiaryUserId,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? null
                : request.DisplayName.Trim(),
            CreatedAtUtc = now
        };

        beneficiaries.Add(beneficiary);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BeneficiaryMapping.ToDto(beneficiary, beneficiaryUser);
    }
}
