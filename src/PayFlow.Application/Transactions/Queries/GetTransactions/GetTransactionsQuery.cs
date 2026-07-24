using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Common.Models;
using PayFlow.Application.Transactions.Dtos;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Transactions.Queries.GetTransactions;

public sealed record GetTransactionsQuery(
    int Page = 1,
    int PageSize = 20,
    TransactionStatus? Status = null,
    TransactionDirection? Direction = null,
    string? ReferenceNumber = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    string Sort = "-createdAt") : IRequest<PagedResult<TransactionDto>>;

public sealed class GetTransactionsQueryHandler(
    ITransactionRepository transactions,
    IWalletRepository wallets,
    ICurrentUser currentUser) : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionDto>>
{
    public async Task<PagedResult<TransactionDto>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var wallet = await wallets.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Wallet was not found for the current user.");

        var sortDescending = !string.Equals(request.Sort, "createdAt", StringComparison.OrdinalIgnoreCase);
        var skip = (request.Page - 1) * request.PageSize;

        var result = await transactions.ListForWalletAsync(
            new TransactionListQuery(
                wallet.Id,
                request.Status,
                request.Direction,
                request.ReferenceNumber,
                request.FromUtc,
                request.ToUtc,
                sortDescending,
                skip,
                request.PageSize),
            cancellationToken);

        var items = result.Items
            .Select(tx => TransactionMapping.ToDto(tx, wallet.Id))
            .ToList();

        return new PagedResult<TransactionDto>(items, request.Page, request.PageSize, result.TotalCount);
    }
}
