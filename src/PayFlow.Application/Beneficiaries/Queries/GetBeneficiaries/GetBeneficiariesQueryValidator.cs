using FluentValidation;

namespace PayFlow.Application.Beneficiaries.Queries.GetBeneficiaries;

public sealed class GetBeneficiariesQueryValidator : AbstractValidator<GetBeneficiariesQuery>
{
    public GetBeneficiariesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
