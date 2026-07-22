using FluentValidation;

namespace PayFlow.Application.Transfers.Commands.TransferMoney;

public sealed class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
{
    public TransferMoneyCommandValidator()
    {
        RuleFor(x => x.ReceiverUserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
