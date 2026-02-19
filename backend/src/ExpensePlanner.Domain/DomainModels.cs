namespace ExpensePlanner.Domain;

/// <summary>
/// Defines whether a transaction represents incoming money or outgoing money.
/// </summary>
public enum TransactionType
{
    Income,
    Expense
}

/// <summary>
/// Defines the time unit for recurrence patterns.
/// </summary>
public enum RecurrenceUnit
{
    Week,
    Month,
    Year
}

/// <summary>
/// Represents a single financial transaction, either in the past (occurred) or future (planned).
/// </summary>
public class Transaction
{
    /// <summary>
    /// Unique identifier for this transaction.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Whether this is income or expense.
    /// </summary>
    public TransactionType Type { get; set; }
    
    /// <summary>
    /// The monetary amount (must be positive).
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// The date this transaction occurred or is planned to occur.
    /// </summary>
    public DateOnly Date { get; set; }
    
    /// <summary>
    /// Optional description of the transaction.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// If this transaction was generated from a recurring transaction template,
    /// this links back to the original recurring transaction.
    /// </summary>
    public Guid? SourceRecurringTransactionId { get; set; }
}

/// <summary>
/// Represents a recurring transaction template that generates occurrences based on a recurrence rule.
/// </summary>
public class RecurringTransaction
{
    /// <summary>
    /// Unique identifier for this recurring transaction.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Whether this generates income or expense transactions.
    /// </summary>
    public TransactionType Type { get; set; }
    
    /// <summary>
    /// The monetary amount for each occurrence (must be positive).
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// The date this recurring pattern starts.
    /// </summary>
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// Reference to the recurrence rule that defines the pattern.
    /// </summary>
    public Guid RecurrenceRuleId { get; set; }
    
    /// <summary>
    /// Optional description for the recurring transaction.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this recurring transaction is currently paused (no new occurrences generated).
    /// </summary>
    public bool IsPaused { get; set; }
}

/// <summary>
/// Defines the recurrence pattern for recurring transactions.
/// DayIndex meaning depends on Unit:
/// - Week: 1..7 (Monday=1, Sunday=7)
/// - Month: 1..28 (day of month, limited to avoid month-end issues)
/// - Year: 1..366 (day of year, handles leap years)
/// </summary>
public class RecurrenceRule
{
    /// <summary>
    /// Unique identifier for this recurrence rule.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// The time unit for recurrence (Week, Month, Year).
    /// </summary>
    public RecurrenceUnit Unit { get; set; }
    
    /// <summary>
    /// How often the recurrence happens (e.g., every 1 week, every 2 months).
    /// Must be positive.
    /// </summary>
    public int Interval { get; set; }
    
    /// <summary>
    /// Specifies which day within the unit:
    /// - Week: 1..7 (Monday=1)
    /// - Month: 1..28 (day of month)
    /// - Year: 1..366 (day of year)
    /// </summary>
    public int DayIndex { get; set; }
}
