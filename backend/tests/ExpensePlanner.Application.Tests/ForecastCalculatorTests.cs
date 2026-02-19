using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.Application.Tests;

public class ForecastCalculatorTests
{
    private readonly ForecastCalculator _calculator = new();

    [Fact]
    public void CalculateDailyBalances_AggregatesOneTimeAndRecurringTransactions()
    {
        var weeklyRule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Week,
            Interval = 1,
            DayIndex = 1 // Monday
        };

        var recurringTransactions = new[]
        {
            new RecurringTransaction
            {
                Id = Guid.NewGuid(),
                Type = TransactionType.Expense,
                Amount = 100m,
                StartDate = new DateOnly(2025, 1, 1),
                RecurrenceRuleId = weeklyRule.Id,
                IsPaused = false
            }
        };

        var transactions = new[]
        {
            MakeTransaction(TransactionType.Income, 1000m, new DateOnly(2025, 1, 1)),
            MakeTransaction(TransactionType.Expense, 200m, new DateOnly(2025, 1, 2)),
            MakeTransaction(TransactionType.Income, 50m, new DateOnly(2025, 1, 5))
        };

        var result = _calculator.CalculateDailyBalances(
            transactions,
            recurringTransactions,
            [weeklyRule],
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 1, 7));

        var expected = new[]
        {
            new DailyBalancePoint(new DateOnly(2025, 1, 1), 1000m, 1000m),
            new DailyBalancePoint(new DateOnly(2025, 1, 2), -200m, 800m),
            new DailyBalancePoint(new DateOnly(2025, 1, 3), 0m, 800m),
            new DailyBalancePoint(new DateOnly(2025, 1, 4), 0m, 800m),
            new DailyBalancePoint(new DateOnly(2025, 1, 5), 50m, 850m),
            new DailyBalancePoint(new DateOnly(2025, 1, 6), -100m, 750m),
            new DailyBalancePoint(new DateOnly(2025, 1, 7), 0m, 750m)
        };

        Assert.Equal(expected, result.DailyBalances);
    }

    [Fact]
    public void CalculateDailyBalances_IncludesHistoryBeforeRangeAsOpeningBalance()
    {
        var transactions = new[]
        {
            MakeTransaction(TransactionType.Income, 100m, new DateOnly(2025, 1, 1)),
            MakeTransaction(TransactionType.Expense, 30m, new DateOnly(2025, 1, 3)),
            MakeTransaction(TransactionType.Income, 40m, new DateOnly(2025, 1, 8))
        };

        var result = _calculator.CalculateDailyBalances(
            transactions,
            [],
            [],
            new DateOnly(2025, 1, 5),
            new DateOnly(2025, 1, 9));

        Assert.Equal(new DailyBalancePoint(new DateOnly(2025, 1, 5), 0m, 70m), result.DailyBalances[0]);
        Assert.Equal(new DailyBalancePoint(new DateOnly(2025, 1, 8), 40m, 110m), result.DailyBalances[3]);
        Assert.Equal(new DailyBalancePoint(new DateOnly(2025, 1, 9), 0m, 110m), result.DailyBalances[4]);
    }

    [Fact]
    public void CalculateBalanceAtDate_WhenTransactionIsRemoved_BalanceChangesAccordingly()
    {
        var date = new DateOnly(2025, 1, 10);

        var withTransaction = new[]
        {
            MakeTransaction(TransactionType.Income, 500m, new DateOnly(2025, 1, 1)),
            MakeTransaction(TransactionType.Expense, 120m, new DateOnly(2025, 1, 5))
        };

        var withoutTransaction = new[]
        {
            MakeTransaction(TransactionType.Income, 500m, new DateOnly(2025, 1, 1))
        };

        var balanceWithExpense = _calculator.CalculateBalanceAtDate(withTransaction, [], [], date);
        var balanceAfterDelete = _calculator.CalculateBalanceAtDate(withoutTransaction, [], [], date);

        Assert.Equal(380m, balanceWithExpense);
        Assert.Equal(500m, balanceAfterDelete);
        Assert.Equal(120m, balanceAfterDelete - balanceWithExpense);
    }

    [Fact]
    public void CalculateDailyBalances_WhenRuleForRecurringTransactionIsMissing_Throws()
    {
        var recurringTransaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Income,
            Amount = 10m,
            StartDate = new DateOnly(2025, 1, 1),
            RecurrenceRuleId = Guid.NewGuid()
        };

        var action = () => _calculator.CalculateDailyBalances(
            [],
            [recurringTransaction],
            [],
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 1, 31));

        Assert.Throws<InvalidOperationException>(action);
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