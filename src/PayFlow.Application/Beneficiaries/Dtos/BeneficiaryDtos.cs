namespace PayFlow.Application.Beneficiaries.Dtos;

public sealed record BeneficiaryDto(
    Guid Id,
    Guid BeneficiaryUserId,
    string Email,
    string FirstName,
    string LastName,
    string? DisplayName,
    DateTime CreatedAtUtc);
