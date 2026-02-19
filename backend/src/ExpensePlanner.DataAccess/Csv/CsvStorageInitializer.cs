namespace ExpensePlanner.DataAccess.Csv;

public class CsvStorageInitializer
{
    public async Task EnsureCreatedAsync(
        string contentRootPath,
        CsvStorageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedOptions = options ?? new CsvStorageOptions();
        var rootPath = CsvStoragePathResolver.ResolveRootPath(contentRootPath, resolvedOptions);

        Directory.CreateDirectory(rootPath);

        foreach (var schema in CsvSchemas.All)
        {
            var filePath = Path.Combine(rootPath, schema.FileName);
            if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                continue;
            }

            var headerLine = CsvRowSerializer.Serialize(schema.Headers);
            await File.WriteAllTextAsync(filePath, headerLine + Environment.NewLine, cancellationToken);
        }
    }
}