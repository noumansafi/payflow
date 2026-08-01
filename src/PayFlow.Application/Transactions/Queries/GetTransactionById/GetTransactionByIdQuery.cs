using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Transactions.Dtos;

namespace PayFlow.Application.Transactions.Queries.GetTransactionById;

public sealed record GetTransactionByIdQuery(Guid TransactionId) : IRequest<TransactionDto>;

public sealed class GetTransactionByIdQueryHandler(
    ITransactionRepository transactions,
    IWalletRepository wallets,
    ICurrentUser currentUser) : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    public async Task<TransactionDto> Handle(
        GetTransactionByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var wallet = await wallets.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Wallet was not found for the current user.");

        var transaction = await transactions.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new NotFoundException("Transaction was not found.");

        if (!TransactionMapping.InvolvesWallet(transaction, wallet.Id))
        {
            throw new NotFoundException("Transaction was not found.");
        }

        var names = await TransactionMapping.ResolveCounterpartyNamesAsync(
            wallets,
            [transaction],
            wallet.Id,
            cancellationToken);

        var counterpartyWalletId = TransactionMapping.CounterpartyWalletId(transaction, wallet.Id);
        names.TryGetValue(counterpartyWalletId, out var name);

        return TransactionMapping.ToDto(transaction, wallet.Id, name);
    }
}
