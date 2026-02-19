namespace ExpensePlanner.Application;

public class DataResetService
{
    private readonly IDataResetRepository _dataResetRepository;

    public DataResetService(IDataResetRepository dataResetRepository)
    {
        _dataResetRepository = dataResetRepository;
    }

    public Task ResetAsync(CancellationToken cancellationToken = default) =>
        _dataResetRepository.ResetAsync(cancellationToken);
}