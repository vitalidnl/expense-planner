namespace ExpensePlanner.DataAccess.Csv;

internal static class CsvFileTable
{
    public static async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ReadRowsAsync(
        string filePath,
        CsvTableSchema schema,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
        if (lines.Length == 0)
        {
            return [];
        }

        var fileHeaders = CsvRowSerializer.Parse(lines[0]);
        var headerIndex = fileHeaders
            .Select((name, index) => new { name, index })
            .ToDictionary(item => item.name, item => item.index, StringComparer.OrdinalIgnoreCase);

        var rows = new List<IReadOnlyDictionary<string, string>>();

        for (var lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = CsvRowSerializer.Parse(line);
            var mapped = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var requiredHeader in schema.Headers)
            {
                if (headerIndex.TryGetValue(requiredHeader, out var valueIndex) && valueIndex < values.Count)
                {
                    mapped[requiredHeader] = values[valueIndex];
                    continue;
                }

                mapped[requiredHeader] = string.Empty;
            }

            rows.Add(mapped);
        }

        return rows;
    }

    public static async Task WriteRowsAtomicAsync(
        string filePath,
        CsvTableSchema schema,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var lines = new List<string>
        {
            CsvRowSerializer.Serialize(schema.Headers)
        };

        foreach (var row in rows)
        {
            var orderedValues = schema.Headers
                .Select(header => row.TryGetValue(header, out var value) ? value : string.Empty)
                .ToArray();

            lines.Add(CsvRowSerializer.Serialize(orderedValues));
        }

        var tempFile = Path.Combine(
            Path.GetDirectoryName(filePath)!,
            $"{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");

        await File.WriteAllLinesAsync(tempFile, lines, cancellationToken);
        File.Move(tempFile, filePath, overwrite: true);
    }
}