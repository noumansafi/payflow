using PayFlow.Domain.Common;
using PayFlow.Domain.Enums;

namespace PayFlow.Domain.Entities;

public sealed class User : AuditableEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;

    public Wallet? Wallet { get; set; }
    public ICollection<Beneficiary> Beneficiaries { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
