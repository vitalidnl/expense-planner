using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Contracts.Transactions;

public sealed record UpdateTransactionRequest(
    TransactionType Type,
    decimal Amount,
    DateOnly Date,
    string? Description);