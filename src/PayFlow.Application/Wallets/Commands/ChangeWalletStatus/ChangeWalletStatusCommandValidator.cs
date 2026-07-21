using FluentValidation;

namespace PayFlow.Application.Wallets.Commands.ChangeWalletStatus;

public sealed class ChangeWalletStatusCommandValidator : AbstractValidator<ChangeWalletStatusCommand>
{
    public ChangeWalletStatusCommandValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
