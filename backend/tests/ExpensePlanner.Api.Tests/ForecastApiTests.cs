using System.Net;
using System.Net.Http.Json;
using ExpensePlanner.Api.Contracts.Forecast;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace ExpensePlanner.Api.Tests;

public sealed class ForecastApiTests
{
    [Fact]
    public async Task GetForecast_ReturnsDailyBalancesFromOneTimeAndRecurringTransactions()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        await SeedForecastFixtureAsync(factory.Services);

        var response = await client.GetAsync("/forecast?from=2025-02-04&to=2025-02-06");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ForecastResponse>();
        Assert.NotNull(payload);
        Assert.Equal(3, payload.DailyBalances.Count);

        var day1 = payload.DailyBalances[0];
        var day2 = payload.DailyBalances[1];
        var day3 = payload.DailyBalances[2];

        Assert.Equal(new DateOnly(2025, 2, 4), day1.Date);
        Assert.Equal(100m, day1.DailyNet);
        Assert.Equal(80m, day1.Balance);

        Assert.Equal(new DateOnly(2025, 2, 5), day2.Date);
        Assert.Equal(-10m, day2.DailyNet);
        Assert.Equal(70m, day2.Balance);

        Assert.Equal(new DateOnly(2025, 2, 6), day3.Date);
        Assert.Equal(0m, day3.DailyNet);
        Assert.Equal(70m, day3.Balance);
    }

    [Fact]
    public async Task GetBalance_ReturnsPredictedBalanceAtDate()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        await SeedForecastFixtureAsync(factory.Services);

        var response = await client.GetAsync("/forecast/balance?date=2025-02-05");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ForecastBalanceResponse>();
        Assert.NotNull(payload);
        Assert.Equal(new DateOnly(2025, 2, 5), payload.Date);
        Assert.Equal(70m, payload.Balance);
    }

    [Fact]
    public async Task GetForecast_WhenFromGreaterThanTo_ReturnsBadRequest()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/forecast?from=2025-02-10&to=2025-02-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task SeedForecastFixtureAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var recurringTransactionRepository = scope.ServiceProvider.GetRequiredService<IRecurringTransactionRepository>();
        var recurrenceRuleRepository = scope.ServiceProvider.GetRequiredService<IRecurrenceRuleRepository>();

        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Month,
            Interval = 1,
            DayIndex = 5
        };

        await recurrenceRuleRepository.AddAsync(rule);

        await transactionRepository.AddAsync(new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Amount = 20m,
            Date = new DateOnly(2025, 2, 3),
            Description = "before range"
        });

        await transactionRepository.AddAsync(new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Income,
            Amount = 100m,
            Date = new DateOnly(2025, 2, 4),
            Description = "inside range"
        });

        await recurringTransactionRepository.AddAsync(new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Amount = 10m,
            StartDate = new DateOnly(2025, 2, 1),
            RecurrenceRuleId = rule.Id,
            Description = "monthly recurring",
            IsPaused = false
        });
    }
}
