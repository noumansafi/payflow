namespace PayFlow.Application.Auth.Dtos;

public sealed record AuthTokensDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc);

public sealed record AuthUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsEmailVerified);

public sealed record AuthResponseDto(
    AuthUserDto User,
    AuthTokensDto Tokens);

public sealed record RegisterResponseDto(
    AuthUserDto User,
    string EmailVerificationToken);
