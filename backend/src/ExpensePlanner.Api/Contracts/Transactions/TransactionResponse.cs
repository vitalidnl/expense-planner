using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Contracts.Transactions;

public sealed record TransactionResponse(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    DateOnly Date,
    string? Description,
    Guid? SourceRecurringTransactionId);