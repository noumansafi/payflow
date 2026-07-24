using FluentValidation;

namespace PayFlow.Application.Transactions.Queries.GetTransactionById;

public sealed class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
    }
}
