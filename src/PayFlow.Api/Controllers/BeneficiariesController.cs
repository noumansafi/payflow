using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.Application.Beneficiaries.Commands.AddBeneficiary;
using PayFlow.Application.Beneficiaries.Commands.RemoveBeneficiary;
using PayFlow.Application.Beneficiaries.Dtos;
using PayFlow.Application.Beneficiaries.Queries.GetBeneficiaries;
using PayFlow.Application.Common.Models;

namespace PayFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/beneficiaries")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class BeneficiariesController(ISender sender) : ControllerBase
{
    /// <summary>List beneficiaries for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BeneficiaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetBeneficiariesQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Add a beneficiary for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(
        [FromBody] AddBeneficiaryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/v1/beneficiaries/{result.Id}", result);
    }

    /// <summary>Remove a beneficiary owned by the authenticated user.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new RemoveBeneficiaryCommand(id), cancellationToken);
        return NoContent();
    }
}
