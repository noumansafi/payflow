using MediatR;
using PayFlow.Application.Auth.Dtos;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Application.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<AuthUserDto>;

public sealed class GetCurrentUserQueryHandler(
    IUserRepository users,
    ICurrentUser currentUser) : IRequestHandler<GetCurrentUserQuery, AuthUserDto>
{
    public async Task<AuthUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException();
        }

        return new AuthUserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.IsEmailVerified);
    }
}
