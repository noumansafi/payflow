namespace PayFlow.Application.Transactions.Dtos;

public sealed record TransactionDto(
    Guid Id,
    string ReferenceNumber,
    string Direction,
    Guid CounterpartyWalletId,
    string? CounterpartyName,
    decimal Amount,
    decimal Fee,
    string Status,
    string TransactionType,
    string? Note,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
