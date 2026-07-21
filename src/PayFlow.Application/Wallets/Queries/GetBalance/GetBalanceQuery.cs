using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Wallets.Dtos;

namespace PayFlow.Application.Wallets.Queries.GetBalance;

public sealed record GetBalanceQuery : IRequest<WalletBalanceDto>;

public sealed class GetBalanceQueryHandler(
    IWalletRepository wallets,
    ICurrentUser currentUser) : IRequestHandler<GetBalanceQuery, WalletBalanceDto>
{
    public async Task<WalletBalanceDto> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var wallet = await wallets.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Wallet was not found for the current user.");

        return new WalletBalanceDto(
            wallet.Id,
            wallet.Balance,
            wallet.Currency,
            wallet.Status.ToString());
    }
}
