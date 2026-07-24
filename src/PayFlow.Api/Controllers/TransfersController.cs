using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.Application.Transfers.Commands.TransferMoney;
using PayFlow.Application.Transfers.Dtos;

namespace PayFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/transfers")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class TransfersController(ISender sender) : ControllerBase
{
    /// <summary>Transfer money from the authenticated user's wallet to another user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransferResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferMoneyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/v1/transactions/by-reference/{result.ReferenceNumber}", result);
    }
}
