using MediatR;
using Microsoft.Extensions.Options;
using PayFlow.Application.Auth.Dtos;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Options;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress = null) : IRequest<AuthResponseDto>;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IAuditLogger auditLogger,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAppException();
        }

        var now = clock.UtcNow;
        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();

        refreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenService.HashToken(refreshToken),
            ExpiresAtUtc = now.AddDays(jwtOptions.Value.RefreshTokenExpirationDays),
            CreatedAtUtc = now,
            CreatedByIp = request.IpAddress
        });

        await auditLogger.WriteAsync(
            AuditAction.Login,
            "User",
            user.Id,
            user.Id,
            """{"event":"login_success"}""",
            request.IpAddress,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            new AuthUserDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role.ToString(),
                user.IsEmailVerified),
            new AuthTokensDto(
                accessToken,
                refreshToken,
                now.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes)));
    }
}
