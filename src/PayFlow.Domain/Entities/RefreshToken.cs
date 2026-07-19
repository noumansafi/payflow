using PayFlow.Domain.Common;

namespace PayFlow.Domain.Entities;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; set; }

    /// <summary>
    /// Store only a hash of the refresh token — never the raw token value.
    /// </summary>
    public required string TokenHash { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public User? User { get; set; }
}
