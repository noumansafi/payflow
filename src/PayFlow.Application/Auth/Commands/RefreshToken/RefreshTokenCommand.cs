using MediatR;
using Microsoft.Extensions.Options;
using PayFlow.Application.Auth.Dtos;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Options;

namespace PayFlow.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null) : IRequest<AuthTokensDto>;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUserRepository users,
    ITokenService tokenService,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions) : IRequestHandler<RefreshTokenCommand, AuthTokensDto>
{
    public async Task<AuthTokensDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var tokenHash = tokenService.HashToken(request.RefreshToken);
        var existing = await refreshTokens.GetByHashAsync(tokenHash, cancellationToken);

        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= now)
        {
            throw new UnauthorizedAppException("Invalid or expired refresh token.");
        }

        var user = await users.GetByIdAsync(existing.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAppException("Invalid or expired refresh token.");
        }

        var newRefreshToken = tokenService.CreateRefreshToken();
        var newRefreshHash = tokenService.HashToken(newRefreshToken);

        existing.RevokedAtUtc = now;
        existing.ReplacedByTokenHash = newRefreshHash;

        refreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshHash,
            ExpiresAtUtc = now.AddDays(jwtOptions.Value.RefreshTokenExpirationDays),
            CreatedAtUtc = now,
            CreatedByIp = request.IpAddress
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokensDto(
            tokenService.CreateAccessToken(user),
            newRefreshToken,
            now.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes));
    }
}
