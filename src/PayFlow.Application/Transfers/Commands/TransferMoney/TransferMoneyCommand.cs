using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Transfers.Dtos;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Transfers;

namespace PayFlow.Application.Transfers.Commands.TransferMoney;

public sealed record TransferMoneyCommand(
    Guid ReceiverUserId,
    decimal Amount,
    string? Note) : IRequest<TransferResultDto>;

public sealed class TransferMoneyCommandHandler(
    IWalletRepository wallets,
    ITransactionRepository transactions,
    INotificationRepository notifications,
    IReferenceNumberGenerator referenceNumbers,
    ICurrentUser currentUser,
    IAuditLogger auditLogger,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<TransferMoneyCommand, TransferResultDto>
{
    public async Task<TransferResultDto> Handle(
        TransferMoneyCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid senderUserId)
        {
            throw new UnauthorizedAppException();
        }

        if (TransferRules.IsSelfTransfer(senderUserId, request.ReceiverUserId))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.ReceiverUserId)] = ["Cannot transfer to yourself."]
            });
        }

        var senderWallet = await wallets.GetByUserIdAsync(senderUserId, cancellationToken)
            ?? throw new NotFoundException("Sender wallet was not found.");

        var receiverWallet = await wallets.GetByUserIdAsync(request.ReceiverUserId, cancellationToken)
            ?? throw new NotFoundException("Receiver wallet was not found.");

        if (!TransferRules.AreWalletsTransferable(senderWallet, receiverWallet))
        {
            throw new ConflictException("Both sender and receiver wallets must be active to transfer.");
        }

        if (!TransferRules.HasSufficientBalance(senderWallet, request.Amount))
        {
            throw new ConflictException("Insufficient wallet balance.");
        }

        var now = clock.UtcNow;
        var referenceNumber = referenceNumbers.Next();
        var fee = 0m;

        senderWallet.Balance -= request.Amount;
        senderWallet.UpdatedAtUtc = now;

        receiverWallet.Balance += request.Amount;
        receiverWallet.UpdatedAtUtc = now;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            SenderWalletId = senderWallet.Id,
            ReceiverWalletId = receiverWallet.Id,
            Amount = request.Amount,
            Fee = fee,
            Status = TransactionStatus.Completed,
            TransactionType = TransactionType.Transfer,
            Note = request.Note,
            CreatedAtUtc = now,
            CompletedAtUtc = now
        };

        transactions.Add(transaction);

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = senderUserId,
            Title = "Transfer sent",
            Body = $"You sent {request.Amount:0.00} {senderWallet.Currency}. Ref: {referenceNumber}",
            Type = NotificationType.TransferSent,
            RelatedEntityId = transaction.Id,
            CreatedAtUtc = now
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.ReceiverUserId,
            Title = "Transfer received",
            Body = $"You received {request.Amount:0.00} {receiverWallet.Currency}. Ref: {referenceNumber}",
            Type = NotificationType.TransferReceived,
            RelatedEntityId = transaction.Id,
            CreatedAtUtc = now
        });

        await auditLogger.WriteAsync(
            AuditAction.Transfer,
            "Transaction",
            transaction.Id,
            senderUserId,
            $$"""{"event":"transfer_completed","referenceNumber":"{{referenceNumber}}","amount":{{request.Amount}},"fee":{{fee}}}""",
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransferResultDto(
            transaction.Id,
            transaction.ReferenceNumber,
            transaction.Amount,
            transaction.Fee,
            transaction.Status.ToString(),
            now);
    }
}
