using MediatR;
using PayFlow.Application.Beneficiaries.Dtos;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Common.Models;

namespace PayFlow.Application.Beneficiaries.Queries.GetBeneficiaries;

public sealed record GetBeneficiariesQuery(
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<BeneficiaryDto>>;

public sealed class GetBeneficiariesQueryHandler(
    IBeneficiaryRepository beneficiaries,
    ICurrentUser currentUser) : IRequestHandler<GetBeneficiariesQuery, PagedResult<BeneficiaryDto>>
{
    public async Task<PagedResult<BeneficiaryDto>> Handle(
        GetBeneficiariesQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid ownerUserId)
        {
            throw new UnauthorizedAppException();
        }

        var skip = (request.Page - 1) * request.PageSize;
        var result = await beneficiaries.ListForOwnerAsync(
            ownerUserId,
            skip,
            request.PageSize,
            cancellationToken);

        var items = result.Items
            .Select(b =>
            {
                var user = b.BeneficiaryUser
                    ?? throw new InvalidOperationException(
                        "Beneficiary user navigation was not loaded.");
                return BeneficiaryMapping.ToDto(b, user);
            })
            .ToList();

        return new PagedResult<BeneficiaryDto>(items, request.Page, request.PageSize, result.TotalCount);
    }
}
