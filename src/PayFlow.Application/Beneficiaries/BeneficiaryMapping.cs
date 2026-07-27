using PayFlow.Application.Beneficiaries.Dtos;
using PayFlow.Domain.Entities;

namespace PayFlow.Application.Beneficiaries;

internal static class BeneficiaryMapping
{
    public static BeneficiaryDto ToDto(Beneficiary beneficiary, User beneficiaryUser) =>
        new(
            beneficiary.Id,
            beneficiary.BeneficiaryUserId,
            beneficiaryUser.Email,
            beneficiaryUser.FirstName,
            beneficiaryUser.LastName,
            beneficiary.DisplayName,
            beneficiary.CreatedAtUtc);
}
