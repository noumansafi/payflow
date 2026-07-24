using FluentValidation;

namespace PayFlow.Application.Transactions.Queries.GetTransactionByReference;

public sealed class GetTransactionByReferenceQueryValidator
    : AbstractValidator<GetTransactionByReferenceQuery>
{
    public GetTransactionByReferenceQueryValidator()
    {
        RuleFor(x => x.ReferenceNumber).NotEmpty().MaximumLength(50);
    }
}
