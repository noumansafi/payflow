using FluentValidation;

namespace PayFlow.Application.Beneficiaries.Commands.RemoveBeneficiary;

public sealed class RemoveBeneficiaryCommandValidator : AbstractValidator<RemoveBeneficiaryCommand>
{
    public RemoveBeneficiaryCommandValidator()
    {
        RuleFor(x => x.BeneficiaryId).NotEmpty();
    }
}
