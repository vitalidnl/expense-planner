using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.Application.Tests;

public class ForecastServiceTests
{
    [Fact]
    public async Task GetForecastAsync_ReturnsDailyBalancesFromRepositories()
    {
        var weeklyRule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Week,
            Interval = 1,
            DayIndex = 1
        };

        var forecastService = new ForecastService(
            new InMemoryTransactionRepository(
            [
                MakeTransaction(TransactionType.Income, 1000m, new DateOnly(2025, 1, 1)),
                MakeTransaction(TransactionType.Expense, 200m, new DateOnly(2025, 1, 2))
            ]),
            new InMemoryRecurringTransactionRepository(
            [
                new RecurringTransaction
                {
                    Id = Guid.NewGuid(),
                    Type = TransactionType.Expense,
                    Amount = 100m,
                    StartDate = new DateOnly(2025, 1, 1),
                    RecurrenceRuleId = weeklyRule.Id
                }
            ]),
            new InMemoryRecurrenceRuleRepository([weeklyRule]));

        var result = await forecastService.GetForecastAsync(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 7));

        Assert.Equal(7, result.DailyBalances.Count);
        Assert.Equal(700m, result.DailyBalances[^1].Balance);
    }

    [Fact]
    public async Task GetBalanceAtDateAsync_ReturnsExpectedBalance()
    {
        var service = new ForecastService(
            new InMemoryTransactionRepository(
            [
                MakeTransaction(TransactionType.Income, 500m, new DateOnly(2025, 1, 1)),
                MakeTransaction(TransactionType.Expense, 120m, new DateOnly(2025, 1, 10))
            ]),
            new InMemoryRecurringTransactionRepository(),
            new InMemoryRecurrenceRuleRepository());

        var balance = await service.GetBalanceAtDateAsync(new DateOnly(2025, 1, 31));

        Assert.Equal(380m, balance);
    }

    private static Transaction MakeTransaction(TransactionType type, decimal amount, DateOnly date) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = type,
            Amount = amount,
            Date = date
        };
}