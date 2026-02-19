using ExpensePlanner.Domain;

namespace ExpensePlanner.Application;

public interface IRecurringTransactionRepository
{
    Task<IReadOnlyList<RecurringTransaction>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RecurringTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(RecurringTransaction recurringTransaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(RecurringTransaction recurringTransaction, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}