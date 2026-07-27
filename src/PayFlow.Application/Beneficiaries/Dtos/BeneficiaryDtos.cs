namespace PayFlow.Application.Beneficiaries.Dtos;

public sealed record BeneficiaryDto(
    Guid Id,
    Guid BeneficiaryUserId,
    string Email,
    string FirstName,
    string LastName,
    string? DisplayName,
    DateTime CreatedAtUtc);

/// <summary>
/// Public candidate returned by lookup before the user confirms Add.
/// </summary>
public sealed record BeneficiaryCandidateDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    bool AlreadySaved);
