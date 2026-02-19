using ExpensePlanner.Application;

namespace ExpensePlanner.DataAccess.Csv;

public class CsvDataResetRepository : IDataResetRepository
{
    private readonly string _contentRootPath;
    private readonly CsvStorageOptions _options;
    private readonly CsvStorageInitializer _initializer;

    public CsvDataResetRepository(
        string contentRootPath,
        CsvStorageOptions? options = null,
        CsvStorageInitializer? initializer = null)
    {
        _contentRootPath = contentRootPath;
        _options = options ?? new CsvStorageOptions();
        _initializer = initializer ?? new CsvStorageInitializer();
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await _initializer.EnsureCreatedAsync(_contentRootPath, _options, cancellationToken);

        foreach (var schema in CsvSchemas.All)
        {
            var filePath = CsvStoragePathResolver.GetFilePath(_contentRootPath, _options, schema);
            await CsvFileTable.WriteRowsAtomicAsync(filePath, schema, [], cancellationToken);
        }
    }
}