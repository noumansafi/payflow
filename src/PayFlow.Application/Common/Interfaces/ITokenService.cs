using PayFlow.Domain.Entities;

namespace PayFlow.Application.Common.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(User user);
    string CreateRefreshToken();
    string HashToken(string rawToken);
}
