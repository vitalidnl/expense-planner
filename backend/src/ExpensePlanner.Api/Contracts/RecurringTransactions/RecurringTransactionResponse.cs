using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Contracts.RecurringTransactions;

public sealed record RecurringTransactionResponse(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    DateOnly StartDate,
    Guid RecurrenceRuleId,
    string? Description,
    bool IsPaused);