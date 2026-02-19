using ExpensePlanner.DataAccess.Csv;
using ExpensePlanner.Domain;

namespace ExpensePlanner.DataAccess.Tests;

public class CsvDataResetRepositoryTests
{
    [Fact]
    public async Task ResetAsync_ClearsAllCsvData()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "expense-planner-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            var options = new CsvStorageOptions { RootPath = "./data" };

            var rule = new RecurrenceRule
            {
                Id = Guid.NewGuid(),
                Unit = RecurrenceUnit.Month,
                Interval = 1,
                DayIndex = 15
            };

            var recurrenceRuleRepository = new CsvRecurrenceRuleRepository(tempRoot, options);
            await recurrenceRuleRepository.AddAsync(rule);

            var recurringTransactionRepository = new CsvRecurringTransactionRepository(tempRoot, options);
            await recurringTransactionRepository.AddAsync(new RecurringTransaction
            {
                Id = Guid.NewGuid(),
                Type = TransactionType.Expense,
                Amount = 49m,
                StartDate = new DateOnly(2025, 1, 1),
                RecurrenceRuleId = rule.Id,
                Description = "Subscription",
                IsPaused = false
            });

            var transactionRepository = new CsvTransactionRepository(tempRoot, options);
            await transactionRepository.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                Type = TransactionType.Income,
                Amount = 250m,
                Date = new DateOnly(2025, 1, 5),
                Description = "Initial balance"
            });

            var resetRepository = new CsvDataResetRepository(tempRoot, options);
            await resetRepository.ResetAsync();

            Assert.Empty(await new CsvTransactionRepository(tempRoot, options).GetAllAsync());
            Assert.Empty(await new CsvRecurringTransactionRepository(tempRoot, options).GetAllAsync());
            Assert.Empty(await new CsvRecurrenceRuleRepository(tempRoot, options).GetAllAsync());
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