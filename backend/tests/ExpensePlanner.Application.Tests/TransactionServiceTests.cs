using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.Application.Tests;

public class TransactionServiceTests
{
    [Fact]
    public async Task GetAsync_FiltersByScopeAndRange()
    {
        var today = new DateOnly(2025, 2, 1);
        var service = new TransactionService(
            new InMemoryTransactionRepository(
            [
                MakeTransaction(new DateOnly(2025, 1, 30), 100m, TransactionType.Income),
                MakeTransaction(new DateOnly(2025, 2, 1), 50m, TransactionType.Expense),
                MakeTransaction(new DateOnly(2025, 2, 3), 75m, TransactionType.Income)
            ]),
            new FakeClock(today));

        var past = await service.GetAsync(scope: TransactionScope.Past);
        var future = await service.GetAsync(scope: TransactionScope.Future);
        var inRange = await service.GetAsync(from: new DateOnly(2025, 2, 1), to: new DateOnly(2025, 2, 2));

        Assert.Equal(2, past.Count);
        Assert.Single(future);
        Assert.Single(inRange);
        Assert.Equal(new DateOnly(2025, 2, 1), inRange[0].Date);
    }

    [Fact]
    public async Task AddAsync_WhenIdIsEmpty_AssignsNewId()
    {
        var repository = new InMemoryTransactionRepository();
        var service = new TransactionService(repository, new FakeClock(new DateOnly(2025, 1, 1)));

        var created = await service.AddAsync(new Transaction
        {
            Id = Guid.Empty,
            Type = TransactionType.Income,
            Amount = 10m,
            Date = new DateOnly(2025, 1, 1)
        });

        Assert.NotEqual(Guid.Empty, created.Id);
        var stored = await repository.GetByIdAsync(created.Id);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task DeleteAsync_WhenIdMissing_Throws()
    {
        var service = new TransactionService(
            new InMemoryTransactionRepository(),
            new FakeClock(new DateOnly(2025, 1, 1)));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    private static Transaction MakeTransaction(DateOnly date, decimal amount, TransactionType type) =>
        new()
        {
            Id = Guid.NewGuid(),
            Date = date,
            Amount = amount,
            Type = type
        };
}