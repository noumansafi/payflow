using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.Application.Wallets.Commands.ChangeWalletStatus;
using PayFlow.Application.Wallets.Commands.CreditWallet;
using PayFlow.Application.Wallets.Dtos;
using PayFlow.Application.Wallets.Queries.GetBalance;
using PayFlow.Application.Wallets.Queries.GetWallet;

namespace PayFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/wallets")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public sealed class WalletsController(
    ISender sender,
    IHostEnvironment environment) : ControllerBase
{
    /// <summary>Get the authenticated user's wallet.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyWallet(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWalletQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get the authenticated user's wallet balance.</summary>
    [HttpGet("me/balance")]
    [ProducesResponseType(typeof(WalletBalanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyBalance(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBalanceQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Change the authenticated user's wallet status (self-service transitions only).</summary>
    [HttpPost("me/status")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeMyWalletStatus(
        [FromBody] ChangeWalletStatusCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Development-only: credit the authenticated user's wallet for local demos.</summary>
    [HttpPost("me/credit")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreditMyWallet(
        [FromBody] CreditWalletCommand command,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
