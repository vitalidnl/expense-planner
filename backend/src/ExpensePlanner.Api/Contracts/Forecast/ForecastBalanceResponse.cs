namespace ExpensePlanner.Api.Contracts.Forecast;

public sealed record ForecastBalanceResponse(
    DateOnly Date,
    decimal Balance);
