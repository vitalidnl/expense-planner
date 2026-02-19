using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.Application.Tests;

public class RecurringTransactionServiceTests
{
    [Fact]
    public async Task AddAsync_WhenRuleExists_CreatesRecurringTransaction()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Month,
            Interval = 1,
            DayIndex = 15
        };

        var recurringRepository = new InMemoryRecurringTransactionRepository();
        var service = new RecurringTransactionService(
            recurringRepository,
            new InMemoryRecurrenceRuleRepository([rule]));

        var created = await service.AddAsync(new RecurringTransaction
        {
            Id = Guid.Empty,
            Type = TransactionType.Expense,
            Amount = 40m,
            StartDate = new DateOnly(2025, 1, 1),
            RecurrenceRuleId = rule.Id,
            IsPaused = false
        });

        Assert.NotEqual(Guid.Empty, created.Id);
        var stored = await recurringRepository.GetByIdAsync(created.Id);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task PauseAndResume_UpdatePauseFlag()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Week,
            Interval = 1,
            DayIndex = 1
        };
        var recurring = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Income,
            Amount = 100m,
            StartDate = new DateOnly(2025, 1, 1),
            RecurrenceRuleId = rule.Id,
            IsPaused = false
        };

        var recurringRepository = new InMemoryRecurringTransactionRepository([recurring]);
        var service = new RecurringTransactionService(
            recurringRepository,
            new InMemoryRecurrenceRuleRepository([rule]));

        await service.PauseAsync(recurring.Id);
        Assert.True((await recurringRepository.GetByIdAsync(recurring.Id))!.IsPaused);

        await service.ResumeAsync(recurring.Id);
        Assert.False((await recurringRepository.GetByIdAsync(recurring.Id))!.IsPaused);
    }

    [Fact]
    public async Task AddAsync_WhenRuleMissing_Throws()
    {
        var service = new RecurringTransactionService(
            new InMemoryRecurringTransactionRepository(),
            new InMemoryRecurrenceRuleRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AddAsync(new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Amount = 10m,
            StartDate = new DateOnly(2025, 1, 1),
            RecurrenceRuleId = Guid.NewGuid()
        }));
    }
}