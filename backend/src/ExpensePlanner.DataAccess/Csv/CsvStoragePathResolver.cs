namespace ExpensePlanner.DataAccess.Csv;

public static class CsvStoragePathResolver
{
    public static string ResolveRootPath(string contentRootPath, CsvStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(contentRootPath))
        {
            throw new ArgumentException("Content root path is required.", nameof(contentRootPath));
        }

        var configuredPath = string.IsNullOrWhiteSpace(options.RootPath)
            ? "./data"
            : options.RootPath;

        return Path.IsPathFullyQualified(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
    }

    public static string GetFilePath(string contentRootPath, CsvStorageOptions options, CsvTableSchema schema)
    {
        var rootPath = ResolveRootPath(contentRootPath, options);
        return Path.Combine(rootPath, schema.FileName);
    }
}