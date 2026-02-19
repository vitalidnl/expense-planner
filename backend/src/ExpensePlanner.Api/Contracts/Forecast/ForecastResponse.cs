namespace ExpensePlanner.Api.Contracts.Forecast;

public sealed record ForecastResponse(
    IReadOnlyList<DailyBalancePointResponse> DailyBalances);
