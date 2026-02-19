using System.Globalization;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.DataAccess.Csv;

public class CsvRecurrenceRuleRepository : IRecurrenceRuleRepository
{
    private readonly string _contentRootPath;
    private readonly CsvStorageOptions _options;
    private readonly CsvStorageInitializer _initializer;

    public CsvRecurrenceRuleRepository(
        string contentRootPath,
        CsvStorageOptions? options = null,
        CsvStorageInitializer? initializer = null)
    {
        _contentRootPath = contentRootPath;
        _options = options ?? new CsvStorageOptions();
        _initializer = initializer ?? new CsvStorageInitializer();
    }

    public async Task<IReadOnlyList<RecurrenceRule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _initializer.EnsureCreatedAsync(_contentRootPath, _options, cancellationToken);
        var rows = await CsvFileTable.ReadRowsAsync(GetFilePath(), CsvSchemas.RecurrenceRules, cancellationToken);
        return rows.Select(MapFromRow).ToList();
    }

    public async Task<RecurrenceRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.SingleOrDefault(item => item.Id == id);
    }

    public async Task AddAsync(RecurrenceRule rule, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        if (all.Any(existing => existing.Id == rule.Id))
        {
            throw new InvalidOperationException($"Recurrence rule '{rule.Id}' already exists.");
        }

        all.Add(rule);
        await PersistAsync(all, cancellationToken);
    }

    public async Task UpdateAsync(RecurrenceRule rule, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        var index = all.FindIndex(existing => existing.Id == rule.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Recurrence rule '{rule.Id}' was not found.");
        }

        all[index] = rule;
        await PersistAsync(all, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        all.RemoveAll(item => item.Id == id);
        await PersistAsync(all, cancellationToken);
    }

    private string GetFilePath() =>
        CsvStoragePathResolver.GetFilePath(_contentRootPath, _options, CsvSchemas.RecurrenceRules);

    private async Task PersistAsync(IReadOnlyList<RecurrenceRule> items, CancellationToken cancellationToken)
    {
        var rows = items.Select(MapToRow).ToList();
        await CsvFileTable.WriteRowsAtomicAsync(GetFilePath(), CsvSchemas.RecurrenceRules, rows, cancellationToken);
    }

    private static RecurrenceRule MapFromRow(IReadOnlyDictionary<string, string> row) =>
        new()
        {
            Id = Guid.Parse(row["Id"]),
            Unit = Enum.Parse<RecurrenceUnit>(row["Unit"], ignoreCase: true),
            Interval = int.Parse(row["Interval"], CultureInfo.InvariantCulture),
            DayIndex = int.Parse(row["DayIndex"], CultureInfo.InvariantCulture)
        };

    private static IReadOnlyDictionary<string, string> MapToRow(RecurrenceRule item) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = item.Id.ToString(),
            ["Unit"] = item.Unit.ToString(),
            ["Interval"] = item.Interval.ToString(CultureInfo.InvariantCulture),
            ["DayIndex"] = item.DayIndex.ToString(CultureInfo.InvariantCulture)
        };
}