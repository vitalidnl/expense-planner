namespace ExpensePlanner.DataAccess.Csv;

public static class CsvSchemas
{
    public static readonly CsvTableSchema Transactions = new(
        "transactions.csv",
        [
            "Id",
            "Type",
            "Amount",
            "Date",
            "Description",
            "SourceRecurringTransactionId"
        ]);

    public static readonly CsvTableSchema RecurringTransactions = new(
        "recurring_transactions.csv",
        [
            "Id",
            "Type",
            "Amount",
            "StartDate",
            "RecurrenceRuleId",
            "Description",
            "IsPaused"
        ]);

    public static readonly CsvTableSchema RecurrenceRules = new(
        "recurrence_rules.csv",
        [
            "Id",
            "Unit",
            "Interval",
            "DayIndex"
        ]);

    public static readonly IReadOnlyList<CsvTableSchema> All =
    [
        Transactions,
        RecurringTransactions,
        RecurrenceRules
    ];
}