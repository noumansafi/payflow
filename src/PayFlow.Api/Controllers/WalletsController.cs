using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.Application.Wallets.Dtos;
using PayFlow.Application.Wallets.Queries.GetBalance;
using PayFlow.Application.Wallets.Queries.GetWallet;

namespace PayFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/wallets")]
[Produces("application/json")]
public sealed class WalletsController(ISender sender) : ControllerBase
{
    /// <summary>Get the authenticated user's wallet.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyWallet(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWalletQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get the authenticated user's wallet balance.</summary>
    [HttpGet("me/balance")]
    [ProducesResponseType(typeof(WalletBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyBalance(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBalanceQuery(), cancellationToken);
        return Ok(result);
    }
}
