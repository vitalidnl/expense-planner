using System.Net;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace ExpensePlanner.Api.Tests;

public sealed class ResetApiTests
{
    [Fact]
    public async Task PostReset_ClearsAllStoredData()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        await SeedDataAsync(factory.Services);

        var resetResponse = await client.PostAsync("/reset", null);

        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var recurringTransactionRepository = scope.ServiceProvider.GetRequiredService<IRecurringTransactionRepository>();
        var recurrenceRuleRepository = scope.ServiceProvider.GetRequiredService<IRecurrenceRuleRepository>();

        var transactions = await transactionRepository.GetAllAsync();
        var recurringTransactions = await recurringTransactionRepository.GetAllAsync();
        var recurrenceRules = await recurrenceRuleRepository.GetAllAsync();

        Assert.Empty(transactions);
        Assert.Empty(recurringTransactions);
        Assert.Empty(recurrenceRules);
    }

    private static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var recurringTransactionRepository = scope.ServiceProvider.GetRequiredService<IRecurringTransactionRepository>();
        var recurrenceRuleRepository = scope.ServiceProvider.GetRequiredService<IRecurrenceRuleRepository>();

        var recurrenceRule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = RecurrenceUnit.Month,
            Interval = 1,
            DayIndex = 10
        };

        await recurrenceRuleRepository.AddAsync(recurrenceRule);

        await transactionRepository.AddAsync(new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Amount = 22m,
            Date = new DateOnly(2025, 2, 1),
            Description = "seed tx"
        });

        await recurringTransactionRepository.AddAsync(new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Income,
            Amount = 500m,
            StartDate = new DateOnly(2025, 2, 1),
            RecurrenceRuleId = recurrenceRule.Id,
            Description = "seed recurring",
            IsPaused = false
        });
    }
}
