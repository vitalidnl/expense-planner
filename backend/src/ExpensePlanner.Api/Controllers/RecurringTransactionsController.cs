using ExpensePlanner.Api.Contracts.RecurringTransactions;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ExpensePlanner.Api.Controllers;

[ApiController]
[Route("recurring-transactions")]
public sealed class RecurringTransactionsController : ControllerBase
{
    private readonly RecurringTransactionService _recurringTransactionService;

    public RecurringTransactionsController(RecurringTransactionService recurringTransactionService)
    {
        _recurringTransactionService = recurringTransactionService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecurringTransactionResponse>>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _recurringTransactionService.GetAsync(cancellationToken);
        return Ok(items.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurringTransactionResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recurringTransaction = await _recurringTransactionService.GetByIdAsync(id, cancellationToken);
        if (recurringTransaction is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(recurringTransaction));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurringTransactionResponse>> CreateAsync(
        [FromBody] CreateRecurringTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _recurringTransactionService.AddAsync(new RecurringTransaction
            {
                Type = request.Type,
                Amount = request.Amount,
                StartDate = request.StartDate,
                RecurrenceRuleId = request.RecurrenceRuleId,
                Description = request.Description,
                IsPaused = request.IsPaused
            }, cancellationToken);

            var response = MapToResponse(created);
            return Created($"/recurring-transactions/{created.Id}", response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateRecurringTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _recurringTransactionService.UpdateAsync(new RecurringTransaction
            {
                Id = id,
                Type = request.Type,
                Amount = request.Amount,
                StartDate = request.StartDate,
                RecurrenceRuleId = request.RecurrenceRuleId,
                Description = request.Description,
                IsPaused = request.IsPaused
            }, cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PauseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _recurringTransactionService.PauseAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResumeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _recurringTransactionService.ResumeAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _recurringTransactionService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private static RecurringTransactionResponse MapToResponse(RecurringTransaction recurringTransaction) =>
        new(
            recurringTransaction.Id,
            recurringTransaction.Type,
            recurringTransaction.Amount,
            recurringTransaction.StartDate,
            recurringTransaction.RecurrenceRuleId,
            recurringTransaction.Description,
            recurringTransaction.IsPaused);
}
