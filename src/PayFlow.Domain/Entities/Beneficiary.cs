using PayFlow.Domain.Common;

namespace PayFlow.Domain.Entities;

public sealed class Beneficiary : AuditableEntity
{
    public Guid OwnerUserId { get; set; }
    public Guid BeneficiaryUserId { get; set; }
    public string? DisplayName { get; set; }

    public User? OwnerUser { get; set; }
    public User? BeneficiaryUser { get; set; }
}
