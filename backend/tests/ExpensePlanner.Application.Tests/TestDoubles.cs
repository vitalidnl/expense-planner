using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.Application.Tests;

internal sealed class FakeClock(DateOnly today) : IClock
{
    public DateOnly Today { get; } = today;
}

internal sealed class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _items;

    public InMemoryTransactionRepository(IEnumerable<Transaction>? seed = null)
    {
        _items = seed?.ToList() ?? [];
    }

    public Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Transaction>>([.. _items]);

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.SingleOrDefault(item => item.Id == id));

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _items.Add(transaction);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(item => item.Id == transaction.Id);
        if (index >= 0)
        {
            _items[index] = transaction;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _items.RemoveAll(item => item.Id == id);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryRecurringTransactionRepository : IRecurringTransactionRepository
{
    private readonly List<RecurringTransaction> _items;

    public InMemoryRecurringTransactionRepository(IEnumerable<RecurringTransaction>? seed = null)
    {
        _items = seed?.ToList() ?? [];
    }

    public Task<IReadOnlyList<RecurringTransaction>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RecurringTransaction>>([.. _items]);

    public Task<RecurringTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.SingleOrDefault(item => item.Id == id));

    public Task AddAsync(RecurringTransaction recurringTransaction, CancellationToken cancellationToken = default)
    {
        _items.Add(recurringTransaction);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RecurringTransaction recurringTransaction, CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(item => item.Id == recurringTransaction.Id);
        if (index >= 0)
        {
            _items[index] = recurringTransaction;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _items.RemoveAll(item => item.Id == id);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryRecurrenceRuleRepository : IRecurrenceRuleRepository
{
    private readonly List<RecurrenceRule> _items;

    public InMemoryRecurrenceRuleRepository(IEnumerable<RecurrenceRule>? seed = null)
    {
        _items = seed?.ToList() ?? [];
    }

    public Task<IReadOnlyList<RecurrenceRule>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RecurrenceRule>>([.. _items]);

    public Task<RecurrenceRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.SingleOrDefault(item => item.Id == id));

    public Task AddAsync(RecurrenceRule rule, CancellationToken cancellationToken = default)
    {
        _items.Add(rule);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RecurrenceRule rule, CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(item => item.Id == rule.Id);
        if (index >= 0)
        {
            _items[index] = rule;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _items.RemoveAll(item => item.Id == id);
        return Task.CompletedTask;
    }
}