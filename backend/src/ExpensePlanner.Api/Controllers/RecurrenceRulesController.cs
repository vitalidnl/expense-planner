using ExpensePlanner.Api.Contracts.RecurrenceRules;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ExpensePlanner.Api.Controllers;

[ApiController]
[Route("recurrence-rules")]
public sealed class RecurrenceRulesController : ControllerBase
{
    private readonly IRecurrenceRuleRepository _recurrenceRuleRepository;

    public RecurrenceRulesController(IRecurrenceRuleRepository recurrenceRuleRepository)
    {
        _recurrenceRuleRepository = recurrenceRuleRepository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecurrenceRuleResponse>>> GetAsync(CancellationToken cancellationToken = default)
    {
        var items = await _recurrenceRuleRepository.GetAllAsync(cancellationToken);
        return Ok(items.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurrenceRuleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recurrenceRule = await _recurrenceRuleRepository.GetByIdAsync(id, cancellationToken);
        if (recurrenceRule is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(recurrenceRule));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<RecurrenceRuleResponse>> CreateAsync(
        [FromBody] CreateRecurrenceRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = request.Unit,
            Interval = request.Interval,
            DayIndex = request.DayIndex
        };

        await _recurrenceRuleRepository.AddAsync(rule, cancellationToken);
        var response = MapToResponse(rule);
        return Created($"/recurrence-rules/{rule.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateRecurrenceRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _recurrenceRuleRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Unit = request.Unit;
        existing.Interval = request.Interval;
        existing.DayIndex = request.DayIndex;

        await _recurrenceRuleRepository.UpdateAsync(existing, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _recurrenceRuleRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static RecurrenceRuleResponse MapToResponse(RecurrenceRule recurrenceRule) =>
        new(
            recurrenceRule.Id,
            recurrenceRule.Unit,
            recurrenceRule.Interval,
            recurrenceRule.DayIndex);
}