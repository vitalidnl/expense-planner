namespace ExpensePlanner.Api.Contracts.Forecast;

public sealed record DailyBalancePointResponse(
    DateOnly Date,
    decimal DailyNet,
    decimal Balance);
