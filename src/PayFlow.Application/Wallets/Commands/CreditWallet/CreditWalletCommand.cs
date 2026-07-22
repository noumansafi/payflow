using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Wallets.Dtos;

namespace PayFlow.Application.Wallets.Commands.CreditWallet;

/// <summary>
/// Development-only funding helper so local/Swagger demos can transfer without SQL.
/// </summary>
public sealed record CreditWalletCommand(decimal Amount) : IRequest<WalletDto>;

public sealed class CreditWalletCommandHandler(
    IWalletRepository wallets,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<CreditWalletCommand, WalletDto>
{
    public async Task<WalletDto> Handle(CreditWalletCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var wallet = await wallets.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Wallet was not found for the current user.");

        wallet.Balance += request.Amount;
        wallet.UpdatedAtUtc = clock.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new WalletDto(
            wallet.Id,
            wallet.UserId,
            wallet.Balance,
            wallet.Currency,
            wallet.Status.ToString(),
            wallet.CreatedAtUtc);
    }
}
