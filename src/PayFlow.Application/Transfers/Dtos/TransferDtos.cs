namespace PayFlow.Application.Transfers.Dtos;

public sealed record TransferResultDto(
    Guid TransactionId,
    string ReferenceNumber,
    decimal Amount,
    decimal Fee,
    string Status,
    DateTime CompletedAtUtc);
