using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Wallets.Dtos;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Wallets;

namespace PayFlow.Application.Wallets.Commands.ChangeWalletStatus;

public sealed record ChangeWalletStatusCommand(WalletStatus Status) : IRequest<WalletDto>;

public sealed class ChangeWalletStatusCommandHandler(
    IWalletRepository wallets,
    ICurrentUser currentUser,
    IAuditLogger auditLogger,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeWalletStatusCommand, WalletDto>
{
    public async Task<WalletDto> Handle(ChangeWalletStatusCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var wallet = await wallets.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Wallet was not found for the current user.");

        if (wallet.Status == request.Status)
        {
            throw new ConflictException($"Wallet is already {request.Status.ToString().ToLowerInvariant()}.");
        }

        if (!WalletStatusTransitions.IsUserAllowed(wallet.Status, request.Status))
        {
            throw new ForbiddenException(
                $"Transition from {wallet.Status} to {request.Status} is not allowed for the wallet owner.");
        }

        wallet.Status = request.Status;
        wallet.UpdatedAtUtc = clock.UtcNow;

        await auditLogger.WriteAsync(
            ResolveAuditAction(request.Status),
            "Wallet",
            wallet.Id,
            userId,
            $$"""{"event":"wallet_status_changed","status":"{{request.Status}}"}""",
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new WalletDto(
            wallet.Id,
            wallet.UserId,
            wallet.Balance,
            wallet.Currency,
            wallet.Status.ToString(),
            wallet.CreatedAtUtc);
    }

    private static AuditAction ResolveAuditAction(WalletStatus status) => status switch
    {
        WalletStatus.Frozen => AuditAction.WalletFreeze,
        WalletStatus.Active => AuditAction.WalletActivation,
        _ => throw new ForbiddenException($"Status {status} is not supported for self-service changes.")
    };
}
