using ExpensePlanner.Domain;

namespace ExpensePlanner.Application;

public interface IRecurrenceRuleRepository
{
    Task<IReadOnlyList<RecurrenceRule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RecurrenceRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(RecurrenceRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(RecurrenceRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}