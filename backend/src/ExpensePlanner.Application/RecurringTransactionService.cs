using ExpensePlanner.Domain;

namespace ExpensePlanner.Application;

public class RecurringTransactionService
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;
    private readonly IRecurrenceRuleRepository _recurrenceRuleRepository;

    public RecurringTransactionService(
        IRecurringTransactionRepository recurringTransactionRepository,
        IRecurrenceRuleRepository recurrenceRuleRepository)
    {
        _recurringTransactionRepository = recurringTransactionRepository;
        _recurrenceRuleRepository = recurrenceRuleRepository;
    }

    public async Task<IReadOnlyList<RecurringTransaction>> GetAsync(CancellationToken cancellationToken = default)
    {
        var all = await _recurringTransactionRepository.GetAllAsync(cancellationToken);
        return all.OrderBy(transaction => transaction.StartDate).ToList();
    }

    public Task<RecurringTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _recurringTransactionRepository.GetByIdAsync(id, cancellationToken);

    public async Task<RecurringTransaction> AddAsync(
        RecurringTransaction recurringTransaction,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecurrenceRuleExistsAsync(recurringTransaction.RecurrenceRuleId, cancellationToken);

        if (recurringTransaction.Id == Guid.Empty)
        {
            recurringTransaction.Id = Guid.NewGuid();
        }

        await _recurringTransactionRepository.AddAsync(recurringTransaction, cancellationToken);
        return recurringTransaction;
    }

    public async Task UpdateAsync(RecurringTransaction recurringTransaction, CancellationToken cancellationToken = default)
    {
        await EnsureRecurringTransactionExistsAsync(recurringTransaction.Id, cancellationToken);
        await EnsureRecurrenceRuleExistsAsync(recurringTransaction.RecurrenceRuleId, cancellationToken);
        await _recurringTransactionRepository.UpdateAsync(recurringTransaction, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureRecurringTransactionExistsAsync(id, cancellationToken);
        await _recurringTransactionRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task PauseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recurringTransaction = await EnsureRecurringTransactionExistsAsync(id, cancellationToken);
        if (recurringTransaction.IsPaused)
        {
            return;
        }

        recurringTransaction.IsPaused = true;
        await _recurringTransactionRepository.UpdateAsync(recurringTransaction, cancellationToken);
    }

    public async Task ResumeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recurringTransaction = await EnsureRecurringTransactionExistsAsync(id, cancellationToken);
        if (!recurringTransaction.IsPaused)
        {
            return;
        }

        recurringTransaction.IsPaused = false;
        await _recurringTransactionRepository.UpdateAsync(recurringTransaction, cancellationToken);
    }

    private async Task<RecurringTransaction> EnsureRecurringTransactionExistsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var existing = await _recurringTransactionRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Recurring transaction '{id}' was not found.");
        }

        return existing;
    }

    private async Task EnsureRecurrenceRuleExistsAsync(Guid recurrenceRuleId, CancellationToken cancellationToken)
    {
        var recurrenceRule = await _recurrenceRuleRepository.GetByIdAsync(recurrenceRuleId, cancellationToken);
        if (recurrenceRule is null)
        {
            throw new KeyNotFoundException($"Recurrence rule '{recurrenceRuleId}' was not found.");
        }
    }
}