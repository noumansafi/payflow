using FluentValidation;
using MediatR;
using PayFlow.Application.Beneficiaries.Dtos;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Beneficiaries;

namespace PayFlow.Application.Beneficiaries.Queries.ResolveBeneficiaryCandidate;

public sealed record ResolveBeneficiaryCandidateQuery(Guid UserId)
    : IRequest<BeneficiaryCandidateDto>;

public sealed class ResolveBeneficiaryCandidateQueryHandler(
    IUserRepository users,
    IBeneficiaryRepository beneficiaries,
    ICurrentUser currentUser) : IRequestHandler<ResolveBeneficiaryCandidateQuery, BeneficiaryCandidateDto>
{
    public async Task<BeneficiaryCandidateDto> Handle(
        ResolveBeneficiaryCandidateQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid ownerUserId)
        {
            throw new UnauthorizedAppException();
        }

        if (BeneficiaryRules.IsSelf(ownerUserId, request.UserId))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.UserId)] = ["Cannot add yourself as a beneficiary."]
            });
        }

        var user = await users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        var alreadySaved = await beneficiaries.ExistsAsync(
            ownerUserId,
            request.UserId,
            cancellationToken);

        return new BeneficiaryCandidateDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            alreadySaved);
    }
}

public sealed class ResolveBeneficiaryCandidateQueryValidator
    : AbstractValidator<ResolveBeneficiaryCandidateQuery>
{
    public ResolveBeneficiaryCandidateQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
