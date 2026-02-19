namespace ExpensePlanner.DataAccess.Csv;

public sealed record CsvTableSchema(
    string FileName,
    IReadOnlyList<string> Headers);