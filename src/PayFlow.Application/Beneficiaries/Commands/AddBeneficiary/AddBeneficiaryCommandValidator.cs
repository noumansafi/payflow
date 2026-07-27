using FluentValidation;

namespace PayFlow.Application.Beneficiaries.Commands.AddBeneficiary;

public sealed class AddBeneficiaryCommandValidator : AbstractValidator<AddBeneficiaryCommand>
{
    public AddBeneficiaryCommandValidator()
    {
        RuleFor(x => x.BeneficiaryUserId).NotEmpty();
        RuleFor(x => x.DisplayName).MaximumLength(150);
    }
}
