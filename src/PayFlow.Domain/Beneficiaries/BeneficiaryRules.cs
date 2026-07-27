namespace PayFlow.Domain.Beneficiaries;

public static class BeneficiaryRules
{
    public static bool IsSelf(Guid ownerUserId, Guid beneficiaryUserId) =>
        ownerUserId == beneficiaryUserId;
}
