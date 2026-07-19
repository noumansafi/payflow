namespace PayFlow.Domain.Enums;

public enum AuditAction
{
    Login = 0,
    Logout = 1,
    PasswordChange = 2,
    Transfer = 3,
    WalletFreeze = 4,
    WalletActivation = 5,
    Register = 6
}
