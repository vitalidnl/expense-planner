using System.Net;
using System.Net.Http.Json;
using ExpensePlanner.Api.Contracts.Transactions;
using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Tests;

public sealed class TransactionsApiTests
{
    [Fact]
    public async Task PostThenGetById_ReturnsCreatedTransaction()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        var createRequest = new CreateTransactionRequest(
            TransactionType.Expense,
            45m,
            new DateOnly(2025, 1, 12),
            "groceries");

        var createResponse = await client.PostAsJsonAsync("/transactions", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(createRequest.Description, fetched.Description);
        Assert.Equal(createRequest.Amount, fetched.Amount);
    }

    [Fact]
    public async Task Get_WithScopeFiltersPastAndFutureRelativeToClock()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/transactions", new CreateTransactionRequest(
            TransactionType.Income,
            100m,
            new DateOnly(2025, 1, 31),
            "past"));

        await client.PostAsJsonAsync("/transactions", new CreateTransactionRequest(
            TransactionType.Expense,
            30m,
            new DateOnly(2025, 2, 1),
            "today"));

        await client.PostAsJsonAsync("/transactions", new CreateTransactionRequest(
            TransactionType.Income,
            75m,
            new DateOnly(2025, 2, 3),
            "future"));

        var allResponse = await client.GetFromJsonAsync<List<TransactionResponse>>("/transactions?scope=all");
        var pastResponse = await client.GetFromJsonAsync<List<TransactionResponse>>("/transactions?scope=past");
        var futureResponse = await client.GetFromJsonAsync<List<TransactionResponse>>("/transactions?scope=future");

        Assert.NotNull(allResponse);
        Assert.NotNull(pastResponse);
        Assert.NotNull(futureResponse);

        Assert.Equal(3, allResponse.Count);
        Assert.Equal(2, pastResponse.Count);
        Assert.Single(futureResponse);
        Assert.Equal(new DateOnly(2025, 2, 3), futureResponse[0].Date);
    }

    [Fact]
    public async Task PutAndDelete_ReturnExpectedStatusCodesAndEffects()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/transactions", new CreateTransactionRequest(
            TransactionType.Expense,
            20m,
            new DateOnly(2025, 2, 1),
            "before update"));

        var created = await createResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.NotNull(created);

        var updateResponse = await client.PutAsJsonAsync($"/transactions/{created.Id}", new UpdateTransactionRequest(
            TransactionType.Income,
            80m,
            new DateOnly(2025, 2, 2),
            "after update"));

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var updated = await client.GetFromJsonAsync<TransactionResponse>($"/transactions/{created.Id}");
        Assert.NotNull(updated);
        Assert.Equal(TransactionType.Income, updated.Type);
        Assert.Equal(80m, updated.Amount);

        var deleteResponse = await client.DeleteAsync($"/transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await client.GetAsync($"/transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Get_WithFromAndTo_FiltersRange()
    {
        using var factory = new TransactionsApiTestFactory();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/transactions", new CreateTransactionRequest(
            TransactionType.Expense,
            10m,
            new DateOnly(2025, 2, 1),
            "one"));

        await client.PostAsJsonAsync("/transactions", new CreateTransactionRequest(
            TransactionType.Expense,
            20m,
            new DateOnly(2025, 2, 2),
            "two"));

        await client.PostAsJsonAsync("/transactions", new CreateTransactionRequest(
            TransactionType.Expense,
            30m,
            new DateOnly(2025, 2, 3),
            "three"));

        var response = await client.GetFromJsonAsync<List<TransactionResponse>>(
            "/transactions?from=2025-02-02&to=2025-02-03");

        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.DoesNotContain(response, item => item.Date < new DateOnly(2025, 2, 2));
        Assert.DoesNotContain(response, item => item.Date > new DateOnly(2025, 2, 3));
    }
}
