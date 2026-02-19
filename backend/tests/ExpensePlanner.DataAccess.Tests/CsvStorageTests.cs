using ExpensePlanner.DataAccess.Csv;

namespace ExpensePlanner.DataAccess.Tests;

public class CsvStorageTests
{
    [Fact]
    public void CsvSchemas_DefineExpectedFilesAndHeaders()
    {
        Assert.Equal("transactions.csv", CsvSchemas.Transactions.FileName);
        Assert.Equal(
            ["Id", "Type", "Amount", "Date", "Description", "SourceRecurringTransactionId"],
            CsvSchemas.Transactions.Headers);

        Assert.Equal("recurring_transactions.csv", CsvSchemas.RecurringTransactions.FileName);
        Assert.Equal(
            ["Id", "Type", "Amount", "StartDate", "RecurrenceRuleId", "Description", "IsPaused"],
            CsvSchemas.RecurringTransactions.Headers);

        Assert.Equal("recurrence_rules.csv", CsvSchemas.RecurrenceRules.FileName);
        Assert.Equal(["Id", "Unit", "Interval", "DayIndex"], CsvSchemas.RecurrenceRules.Headers);
    }

    [Fact]
    public void CsvRowSerializer_RoundTripsQuotedAndCommaValues()
    {
        var values = new[] { "Id", "salary,monthly", "quote \"value\"", "plain" };

        var serialized = CsvRowSerializer.Serialize(values);
        var parsed = CsvRowSerializer.Parse(serialized);

        Assert.Equal(values, parsed);
    }

    [Fact]
    public void CsvStoragePathResolver_ResolvesRelativeRootAgainstContentRoot()
    {
        var options = new CsvStorageOptions { RootPath = "./data" };
        var root = CsvStoragePathResolver.ResolveRootPath("C:/repo/backend/src/ExpensePlanner.Api", options);

        Assert.EndsWith("backend/src/ExpensePlanner.Api/data", root.Replace('\\', '/'));
    }

    [Fact]
    public async Task CsvStorageInitializer_CreatesCsvFilesWithHeadersOnFirstRun()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "expense-planner-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            var options = new CsvStorageOptions { RootPath = "./data" };
            var initializer = new CsvStorageInitializer();

            await initializer.EnsureCreatedAsync(tempRoot, options);

            var csvRoot = Path.Combine(tempRoot, "data");
            foreach (var schema in CsvSchemas.All)
            {
                var filePath = Path.Combine(csvRoot, schema.FileName);
                Assert.True(File.Exists(filePath), $"Expected file '{filePath}' to exist.");

                var firstLine = (await File.ReadAllLinesAsync(filePath))[0];
                Assert.Equal(CsvRowSerializer.Serialize(schema.Headers), firstLine);
            }
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}