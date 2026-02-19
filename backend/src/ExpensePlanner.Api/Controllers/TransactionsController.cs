using ExpensePlanner.Api.Contracts.Transactions;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ExpensePlanner.Api.Controllers;

[ApiController]
[Route("transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> GetAsync(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] TransactionScope scope = TransactionScope.All,
        CancellationToken cancellationToken = default)
    {
        var items = await _transactionService.GetAsync(from, to, scope, cancellationToken);
        return Ok(items.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionService.GetByIdAsync(id, cancellationToken);
        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(transaction));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TransactionResponse>> CreateAsync(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await _transactionService.AddAsync(new Transaction
        {
            Type = request.Type,
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description
        }, cancellationToken);

        var response = MapToResponse(created);
        return Created($"/transactions/{created.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _transactionService.UpdateAsync(new Transaction
            {
                Id = id,
                Type = request.Type,
                Amount = request.Amount,
                Date = request.Date,
                Description = request.Description
            }, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _transactionService.DeleteAsync(id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static TransactionResponse MapToResponse(Transaction transaction) =>
        new(
            transaction.Id,
            transaction.Type,
            transaction.Amount,
            transaction.Date,
            transaction.Description,
            transaction.SourceRecurringTransactionId);
}
