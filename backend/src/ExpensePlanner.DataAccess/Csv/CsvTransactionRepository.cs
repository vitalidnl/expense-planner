using System.Globalization;
using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.DataAccess.Csv;

public class CsvTransactionRepository : ITransactionRepository
{
    private readonly string _contentRootPath;
    private readonly CsvStorageOptions _options;
    private readonly CsvStorageInitializer _initializer;

    public CsvTransactionRepository(
        string contentRootPath,
        CsvStorageOptions? options = null,
        CsvStorageInitializer? initializer = null)
    {
        _contentRootPath = contentRootPath;
        _options = options ?? new CsvStorageOptions();
        _initializer = initializer ?? new CsvStorageInitializer();
    }

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _initializer.EnsureCreatedAsync(_contentRootPath, _options, cancellationToken);
        var rows = await CsvFileTable.ReadRowsAsync(GetFilePath(), CsvSchemas.Transactions, cancellationToken);

        return rows.Select(MapFromRow).ToList();
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.SingleOrDefault(transaction => transaction.Id == id);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        if (all.Any(existing => existing.Id == transaction.Id))
        {
            throw new InvalidOperationException($"Transaction '{transaction.Id}' already exists.");
        }

        all.Add(transaction);
        await PersistAsync(all, cancellationToken);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        var index = all.FindIndex(existing => existing.Id == transaction.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Transaction '{transaction.Id}' was not found.");
        }

        all[index] = transaction;
        await PersistAsync(all, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var all = (await GetAllAsync(cancellationToken)).ToList();
        all.RemoveAll(transaction => transaction.Id == id);
        await PersistAsync(all, cancellationToken);
    }

    private string GetFilePath() =>
        CsvStoragePathResolver.GetFilePath(_contentRootPath, _options, CsvSchemas.Transactions);

    private async Task PersistAsync(IReadOnlyList<Transaction> transactions, CancellationToken cancellationToken)
    {
        var rows = transactions.Select(MapToRow).ToList();
        await CsvFileTable.WriteRowsAtomicAsync(GetFilePath(), CsvSchemas.Transactions, rows, cancellationToken);
    }

    private static Transaction MapFromRow(IReadOnlyDictionary<string, string> row)
    {
        Guid? sourceRecurringId = string.IsNullOrWhiteSpace(row["SourceRecurringTransactionId"])
            ? null
            : Guid.Parse(row["SourceRecurringTransactionId"]);

        return new Transaction
        {
            Id = Guid.Parse(row["Id"]),
            Type = Enum.Parse<TransactionType>(row["Type"], ignoreCase: true),
            Amount = decimal.Parse(row["Amount"], CultureInfo.InvariantCulture),
            Date = DateOnly.ParseExact(row["Date"], "yyyy-MM-dd", CultureInfo.InvariantCulture),
            Description = string.IsNullOrWhiteSpace(row["Description"]) ? null : row["Description"],
            SourceRecurringTransactionId = sourceRecurringId
        };
    }

    private static IReadOnlyDictionary<string, string> MapToRow(Transaction transaction) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = transaction.Id.ToString(),
            ["Type"] = transaction.Type.ToString(),
            ["Amount"] = transaction.Amount.ToString(CultureInfo.InvariantCulture),
            ["Date"] = transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["Description"] = transaction.Description ?? string.Empty,
            ["SourceRecurringTransactionId"] = transaction.SourceRecurringTransactionId?.ToString() ?? string.Empty
        };
}