namespace ExpensePlanner.Application;

public interface IDataResetRepository
{
    Task ResetAsync(CancellationToken cancellationToken = default);
}