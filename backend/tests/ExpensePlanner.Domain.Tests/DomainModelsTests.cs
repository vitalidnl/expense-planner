using ExpensePlanner.Domain;

namespace ExpensePlanner.Domain.Tests;

public class DomainModelsTests
{
    [Fact]
    public void Transaction_CanBeCreated_WithValidProperties()
    {
        // Arrange & Act
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Income,
            Amount = 100.50m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Description = "Test transaction"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(100.50m, transaction.Amount);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), transaction.Date);
        Assert.Equal("Test transaction", transaction.Description);
        Assert.Null(transaction.SourceRecurringTransactionId);
    }

    [Fact]
    public void RecurringTransaction_CanBeCreated_WithValidProperties()
    {
        // Arrange & Act
        var recurringTransaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Amount = 50.00m,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            RecurrenceRuleId = Guid.NewGuid(),
            Description = "Monthly subscription",
            IsPaused = false
        };

        // Assert
        Assert.NotEqual(Guid.Empty, recurringTransaction.Id);
        Assert.Equal(TransactionType.Expense, recurringTransaction.Type);
        Assert.Equal(50.00m, recurringTransaction.Amount);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), recurringTransaction.StartDate);
        Assert.NotEqual(Guid.Empty, recurringTransaction.RecurrenceRuleId);
        Assert.Equal("Monthly subscription", recurringTransaction.Description);
        Assert.False(recurringTransaction.IsPaused);
    }

    [Fact]
    public void RecurrenceRule_CanBeCreated_WithValidWeeklyPattern()
    {
        // Arrange & Act
        var recurrenceRule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Week,
            Interval = 1,
            DayIndex = 1 // Monday
        };

        // Assert
        Assert.NotEqual(Guid.Empty, recurrenceRule.Id);
        Assert.Equal(RecurrenceUnit.Week, recurrenceRule.Unit);
        Assert.Equal(1, recurrenceRule.Interval);
        Assert.Equal(1, recurrenceRule.DayIndex);
    }

    [Fact]
    public void RecurrenceRule_CanBeCreated_WithValidMonthlyPattern()
    {
        // Arrange & Act
        var recurrenceRule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Month,
            Interval = 2,
            DayIndex = 15 // 15th of month
        };

        // Assert
        Assert.NotEqual(Guid.Empty, recurrenceRule.Id);
        Assert.Equal(RecurrenceUnit.Month, recurrenceRule.Unit);
        Assert.Equal(2, recurrenceRule.Interval);
        Assert.Equal(15, recurrenceRule.DayIndex);
    }

    [Fact]
    public void RecurrenceRule_CanBeCreated_WithValidYearlyPattern()
    {
        // Arrange & Act
        var recurrenceRule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Year,
            Interval = 1,
            DayIndex = 100 // 100th day of year
        };

        // Assert
        Assert.NotEqual(Guid.Empty, recurrenceRule.Id);
        Assert.Equal(RecurrenceUnit.Year, recurrenceRule.Unit);
        Assert.Equal(1, recurrenceRule.Interval);
        Assert.Equal(100, recurrenceRule.DayIndex);
    }

    [Theory]
    [InlineData(TransactionType.Income)]
    [InlineData(TransactionType.Expense)]
    public void TransactionType_AllValidValuesWork(TransactionType type)
    {
        // Arrange & Act
        var transaction = new Transaction { Type = type };

        // Assert
        Assert.Equal(type, transaction.Type);
    }

    [Theory]
    [InlineData(RecurrenceUnit.Week)]
    [InlineData(RecurrenceUnit.Month)]
    [InlineData(RecurrenceUnit.Year)]
    public void RecurrenceUnit_AllValidValuesWork(RecurrenceUnit unit)
    {
        // Arrange & Act
        var rule = new RecurrenceRule { Unit = unit };

        // Assert
        Assert.Equal(unit, rule.Unit);
    }
}
