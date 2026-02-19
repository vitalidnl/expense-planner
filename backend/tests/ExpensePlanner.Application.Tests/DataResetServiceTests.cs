using ExpensePlanner.Application;

namespace ExpensePlanner.Application.Tests;

public class DataResetServiceTests
{
    [Fact]
    public async Task ResetAsync_DelegatesToRepository()
    {
        var fakeRepository = new FakeDataResetRepository();
        var service = new DataResetService(fakeRepository);

        await service.ResetAsync();

        Assert.True(fakeRepository.WasCalled);
    }

    private sealed class FakeDataResetRepository : IDataResetRepository
    {
        public bool WasCalled { get; private set; }

        public Task ResetAsync(CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }
}