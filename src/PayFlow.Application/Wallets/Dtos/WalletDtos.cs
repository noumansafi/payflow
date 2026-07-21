namespace PayFlow.Application.Wallets.Dtos;

public sealed record WalletDto(
    Guid Id,
    Guid UserId,
    decimal Balance,
    string Currency,
    string Status,
    DateTime CreatedAtUtc);

public sealed record WalletBalanceDto(
    Guid WalletId,
    decimal Balance,
    string Currency,
    string Status);
