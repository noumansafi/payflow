using FluentValidation;

namespace PayFlow.Application.Transactions.Queries.GetTransactions;

public sealed class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
{
    private static readonly string[] AllowedSorts = ["createdAt", "-createdAt"];

    public GetTransactionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ReferenceNumber).MaximumLength(50);
        RuleFor(x => x.Sort)
            .Must(sort => AllowedSorts.Contains(sort, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Sort must be 'createdAt' or '-createdAt'.");
        RuleFor(x => x)
            .Must(x => x.FromUtc is null || x.ToUtc is null || x.FromUtc <= x.ToUtc)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");
    }
}
