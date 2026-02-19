using ExpensePlanner.DataAccess.Csv;
using ExpensePlanner.Domain;

namespace ExpensePlanner.DataAccess.Tests;

public class CsvRepositoriesTests
{
    [Fact]
    public async Task TransactionRepository_CrudAndPersistenceAcrossInstances()
    {
        await WithTempRootAsync(async tempRoot =>
        {
            var options = new CsvStorageOptions { RootPath = "./data" };
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Type = TransactionType.Income,
                Amount = 100m,
                Date = new DateOnly(2025, 1, 10),
                Description = "Salary"
            };

            var repository = new CsvTransactionRepository(tempRoot, options);
            await repository.AddAsync(transaction);

            var reloaded = new CsvTransactionRepository(tempRoot, options);
            var stored = await reloaded.GetByIdAsync(transaction.Id);
            Assert.NotNull(stored);
            Assert.Equal("Salary", stored!.Description);

            stored.Description = "Updated Salary";
            stored.Amount = 150m;
            await reloaded.UpdateAsync(stored);

            var updated = await new CsvTransactionRepository(tempRoot, options).GetByIdAsync(transaction.Id);
            Assert.NotNull(updated);
            Assert.Equal(150m, updated!.Amount);
            Assert.Equal("Updated Salary", updated.Description);

            await reloaded.DeleteAsync(transaction.Id);
            var deleted = await new CsvTransactionRepository(tempRoot, options).GetByIdAsync(transaction.Id);
            Assert.Null(deleted);
        });
    }

    [Fact]
    public async Task RecurringTransactionRepository_CrudAndStableId()
    {
        await WithTempRootAsync(async tempRoot =>
        {
            var options = new CsvStorageOptions { RootPath = "./data" };
            var item = new RecurringTransaction
            {
                Id = Guid.NewGuid(),
                Type = TransactionType.Expense,
                Amount = 25m,
                StartDate = new DateOnly(2025, 1, 1),
                RecurrenceRuleId = Guid.NewGuid(),
                Description = "Subscription",
                IsPaused = false
            };

            var repository = new CsvRecurringTransactionRepository(tempRoot, options);
            await repository.AddAsync(item);

            var reloaded = new CsvRecurringTransactionRepository(tempRoot, options);
            var stored = await reloaded.GetByIdAsync(item.Id);
            Assert.NotNull(stored);
            Assert.Equal(item.Id, stored!.Id);

            stored.IsPaused = true;
            await reloaded.UpdateAsync(stored);

            var updated = await new CsvRecurringTransactionRepository(tempRoot, options).GetByIdAsync(item.Id);
            Assert.NotNull(updated);
            Assert.True(updated!.IsPaused);

            await reloaded.DeleteAsync(item.Id);
            var deleted = await new CsvRecurringTransactionRepository(tempRoot, options).GetByIdAsync(item.Id);
            Assert.Null(deleted);
        });
    }

    [Fact]
    public async Task RecurrenceRuleRepository_CrudAndPersistenceAcrossInstances()
    {
        await WithTempRootAsync(async tempRoot =>
        {
            var options = new CsvStorageOptions { RootPath = "./data" };
            var rule = new RecurrenceRule
            {
                Id = Guid.NewGuid(),
                Unit = RecurrenceUnit.Month,
                Interval = 2,
                DayIndex = 12
            };

            var repository = new CsvRecurrenceRuleRepository(tempRoot, options);
            await repository.AddAsync(rule);

            var reloaded = new CsvRecurrenceRuleRepository(tempRoot, options);
            var stored = await reloaded.GetByIdAsync(rule.Id);
            Assert.NotNull(stored);
            Assert.Equal(2, stored!.Interval);

            stored.Interval = 3;
            await reloaded.UpdateAsync(stored);

            var updated = await new CsvRecurrenceRuleRepository(tempRoot, options).GetByIdAsync(rule.Id);
            Assert.NotNull(updated);
            Assert.Equal(3, updated!.Interval);

            await reloaded.DeleteAsync(rule.Id);
            var deleted = await new CsvRecurrenceRuleRepository(tempRoot, options).GetByIdAsync(rule.Id);
            Assert.Null(deleted);
        });
    }

    private static async Task WithTempRootAsync(Func<string, Task> action)
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "expense-planner-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            await action(tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}