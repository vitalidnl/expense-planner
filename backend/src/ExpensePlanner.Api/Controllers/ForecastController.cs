using ExpensePlanner.Api.Contracts.Forecast;
using ExpensePlanner.Application;
using Microsoft.AspNetCore.Mvc;

namespace ExpensePlanner.Api.Controllers;

[ApiController]
[Route("forecast")]
public sealed class ForecastController : ControllerBase
{
    private readonly ForecastService _forecastService;

    public ForecastController(ForecastService forecastService)
    {
        _forecastService = forecastService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ForecastResponse>> GetAsync(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        if (!from.HasValue || !to.HasValue)
        {
            return BadRequest("Query parameters 'from' and 'to' are required.");
        }

        if (from.Value > to.Value)
        {
            return BadRequest("Query parameter 'from' must be less than or equal to 'to'.");
        }

        var forecast = await _forecastService.GetForecastAsync(from.Value, to.Value, cancellationToken);
        var response = new ForecastResponse(
            forecast.DailyBalances
                .Select(point => new DailyBalancePointResponse(point.Date, point.DailyNet, point.Balance))
                .ToList());

        return Ok(response);
    }

    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ForecastBalanceResponse>> GetBalanceAsync(
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken = default)
    {
        if (!date.HasValue)
        {
            return BadRequest("Query parameter 'date' is required.");
        }

        var balance = await _forecastService.GetBalanceAtDateAsync(date.Value, cancellationToken);
        return Ok(new ForecastBalanceResponse(date.Value, balance));
    }
}
