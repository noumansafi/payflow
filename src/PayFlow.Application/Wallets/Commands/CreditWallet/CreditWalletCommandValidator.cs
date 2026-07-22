using FluentValidation;

namespace PayFlow.Application.Wallets.Commands.CreditWallet;

public sealed class CreditWalletCommandValidator : AbstractValidator<CreditWalletCommand>
{
    public CreditWalletCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0m);
    }
}
