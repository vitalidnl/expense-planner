using System.Globalization;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.DataAccess.Csv;

public class CsvRecurringTransactionRepository : IRecurringTransactionRepository
{
    private readonly string _contentRootPath;
    private readonly CsvStorageOptions _options;
    private readonly CsvStorageInitializer _initializer;

    public CsvRecurringTransactionRepository(
        string contentRootPath,
        CsvStorageOptions? options = null,
        CsvStorageInitializer? initializer = null)
    {
        _contentRootPath = contentRootPath;
        _options = options ?? new CsvStorageOptions();
        _initializer = initializer ?? new CsvStorageInitializer();
    }

    public async Task<IReadOnlyList<RecurringTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _initializer.EnsureCreatedAsync(_contentRootPath, _options, cancellationToken);
        var rows = await CsvFileTable.ReadRowsAsync(GetFilePath(), CsvSchemas.RecurringTransactions, cancellationToken);
        return rows.Select(MapFromRow).ToList();
    }

    public async Task<RecurringTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.SingleOrDefault(item => item.Id == id);
    }

    public async Task AddAsync(RecurringTransaction recurringTransaction, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        if (all.Any(existing => existing.Id == recurringTransaction.Id))
        {
            throw new InvalidOperationException($"Recurring transaction '{recurringTransaction.Id}' already exists.");
        }

        all.Add(recurringTransaction);
        await PersistAsync(all, cancellationToken);
    }

    public async Task UpdateAsync(RecurringTransaction recurringTransaction, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        var index = all.FindIndex(existing => existing.Id == recurringTransaction.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Recurring transaction '{recurringTransaction.Id}' was not found.");
        }

        all[index] = recurringTransaction;
        await PersistAsync(all, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        all.RemoveAll(item => item.Id == id);
        await PersistAsync(all, cancellationToken);
    }

    private string GetFilePath() =>
        CsvStoragePathResolver.GetFilePath(_contentRootPath, _options, CsvSchemas.RecurringTransactions);

    private async Task PersistAsync(IReadOnlyList<RecurringTransaction> items, CancellationToken cancellationToken)
    {
        var rows = items.Select(MapToRow).ToList();
        await CsvFileTable.WriteRowsAtomicAsync(GetFilePath(), CsvSchemas.RecurringTransactions, rows, cancellationToken);
    }

    private static RecurringTransaction MapFromRow(IReadOnlyDictionary<string, string> row) =>
        new()
        {
            Id = Guid.Parse(row["Id"]),
            Type = Enum.Parse<TransactionType>(row["Type"], ignoreCase: true),
            Amount = decimal.Parse(row["Amount"], CultureInfo.InvariantCulture),
            StartDate = DateOnly.ParseExact(row["StartDate"], "yyyy-MM-dd", CultureInfo.InvariantCulture),
            RecurrenceRuleId = Guid.Parse(row["RecurrenceRuleId"]),
            Description = string.IsNullOrWhiteSpace(row["Description"]) ? null : row["Description"],
            IsPaused = bool.Parse(row["IsPaused"])
        };

    private static IReadOnlyDictionary<string, string> MapToRow(RecurringTransaction item) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = item.Id.ToString(),
            ["Type"] = item.Type.ToString(),
            ["Amount"] = item.Amount.ToString(CultureInfo.InvariantCulture),
            ["StartDate"] = item.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["RecurrenceRuleId"] = item.RecurrenceRuleId.ToString(),
            ["Description"] = item.Description ?? string.Empty,
            ["IsPaused"] = item.IsPaused.ToString()
        };
}