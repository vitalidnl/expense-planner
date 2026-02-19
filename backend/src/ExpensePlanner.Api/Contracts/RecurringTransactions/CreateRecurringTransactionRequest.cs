using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Contracts.RecurringTransactions;

public sealed record CreateRecurringTransactionRequest(
    TransactionType Type,
    decimal Amount,
    DateOnly StartDate,
    Guid RecurrenceRuleId,
    string? Description,
    bool IsPaused);