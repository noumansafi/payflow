using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.Application.Common.Models;
using PayFlow.Application.Transactions.Dtos;
using PayFlow.Application.Transactions.Queries.GetTransactionById;
using PayFlow.Application.Transactions.Queries.GetTransactionByReference;
using PayFlow.Application.Transactions.Queries.GetTransactions;
using PayFlow.Domain.Enums;

namespace PayFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/transactions")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class TransactionsController(ISender sender) : ControllerBase
{
    /// <summary>List transactions for the authenticated user's wallet.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TransactionStatus? status = null,
        [FromQuery] TransactionDirection? direction = null,
        [FromQuery] string? referenceNumber = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] string sort = "-createdAt",
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetTransactionsQuery(
                page,
                pageSize,
                status,
                direction,
                referenceNumber,
                fromUtc,
                toUtc,
                sort),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Get a transaction by id (only if it involves the current user's wallet).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTransactionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a transaction by reference number (only if it involves the current user's wallet).</summary>
    [HttpGet("by-reference/{referenceNumber}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference(
        string referenceNumber,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTransactionByReferenceQuery(referenceNumber),
            cancellationToken);
        return Ok(result);
    }
}
