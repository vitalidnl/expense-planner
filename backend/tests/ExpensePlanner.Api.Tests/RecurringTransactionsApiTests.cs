using System.Net;
using System.Net.Http.Json;
using ExpensePlanner.Api.Contracts.RecurringTransactions;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace ExpensePlanner.Api.Tests;

public sealed class RecurringTransactionsApiTests
{
    [Fact]
    public async Task PostThenGetById_ReturnsCreatedRecurringTransaction()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        var recurrenceRuleId = await AddRecurrenceRuleAsync(factory.Services);

        var createResponse = await client.PostAsJsonAsync("/recurring-transactions", new CreateRecurringTransactionRequest(
            TransactionType.Expense,
            35m,
            new DateOnly(2025, 2, 1),
            recurrenceRuleId,
            "gym",
            false));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<RecurringTransactionResponse>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/recurring-transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<RecurringTransactionResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(recurrenceRuleId, fetched.RecurrenceRuleId);
    }

    [Fact]
    public async Task PauseAndResume_UpdatesPausedState()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        var recurrenceRuleId = await AddRecurrenceRuleAsync(factory.Services);
        var created = await CreateRecurringTransactionAsync(client, recurrenceRuleId);

        var pauseResponse = await client.PostAsync($"/recurring-transactions/{created.Id}/pause", null);
        Assert.Equal(HttpStatusCode.NoContent, pauseResponse.StatusCode);

        var paused = await client.GetFromJsonAsync<RecurringTransactionResponse>($"/recurring-transactions/{created.Id}");
        Assert.NotNull(paused);
        Assert.True(paused.IsPaused);

        var resumeResponse = await client.PostAsync($"/recurring-transactions/{created.Id}/resume", null);
        Assert.Equal(HttpStatusCode.NoContent, resumeResponse.StatusCode);

        var resumed = await client.GetFromJsonAsync<RecurringTransactionResponse>($"/recurring-transactions/{created.Id}");
        Assert.NotNull(resumed);
        Assert.False(resumed.IsPaused);
    }

    [Fact]
    public async Task PutAndDelete_ReturnExpectedStatusCodesAndEffects()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        var firstRuleId = await AddRecurrenceRuleAsync(factory.Services, RecurrenceUnit.Month, 1, 15);
        var secondRuleId = await AddRecurrenceRuleAsync(factory.Services, RecurrenceUnit.Year, 1, 200);

        var created = await CreateRecurringTransactionAsync(client, firstRuleId);

        var updateResponse = await client.PutAsJsonAsync($"/recurring-transactions/{created.Id}", new UpdateRecurringTransactionRequest(
            TransactionType.Income,
            1200m,
            new DateOnly(2025, 3, 1),
            secondRuleId,
            "updated recurring",
            true));

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var updated = await client.GetFromJsonAsync<RecurringTransactionResponse>($"/recurring-transactions/{created.Id}");
        Assert.NotNull(updated);
        Assert.Equal(TransactionType.Income, updated.Type);
        Assert.Equal(1200m, updated.Amount);
        Assert.Equal(secondRuleId, updated.RecurrenceRuleId);
        Assert.True(updated.IsPaused);

        var deleteResponse = await client.DeleteAsync($"/recurring-transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await client.GetAsync($"/recurring-transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingRecurrenceRule_ReturnsNotFound()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/recurring-transactions", new CreateRecurringTransactionRequest(
            TransactionType.Expense,
            15m,
            new DateOnly(2025, 2, 1),
            Guid.NewGuid(),
            "invalid",
            false));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<Guid> AddRecurrenceRuleAsync(
        IServiceProvider serviceProvider,
        RecurrenceUnit unit = RecurrenceUnit.Month,
        int interval = 1,
        int dayIndex = 10)
    {
        using var scope = serviceProvider.CreateScope();
        var recurrenceRuleRepository = scope.ServiceProvider.GetRequiredService<IRecurrenceRuleRepository>();

        var recurrenceRule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Unit = unit,
            Interval = interval,
            DayIndex = dayIndex
        };

        await recurrenceRuleRepository.AddAsync(recurrenceRule);
        return recurrenceRule.Id;
    }

    private static async Task<RecurringTransactionResponse> CreateRecurringTransactionAsync(HttpClient client, Guid recurrenceRuleId)
    {
        var createResponse = await client.PostAsJsonAsync("/recurring-transactions", new CreateRecurringTransactionRequest(
            TransactionType.Expense,
            60m,
            new DateOnly(2025, 2, 1),
            recurrenceRuleId,
            "base recurring",
            false));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<RecurringTransactionResponse>();
        Assert.NotNull(created);
        return created;
    }
}
