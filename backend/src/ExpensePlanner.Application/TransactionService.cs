using ExpensePlanner.Domain;

namespace ExpensePlanner.Application;

public enum TransactionScope
{
    All,
    Past,
    Future
}

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IClock _clock;

    public TransactionService(ITransactionRepository transactionRepository, IClock clock)
    {
        _transactionRepository = transactionRepository;
        _clock = clock;
    }

    public async Task<IReadOnlyList<Transaction>> GetAsync(
        DateOnly? from = null,
        DateOnly? to = null,
        TransactionScope scope = TransactionScope.All,
        CancellationToken cancellationToken = default)
    {
        var all = await _transactionRepository.GetAllAsync(cancellationToken);
        var today = _clock.Today;

        return all
            .Where(transaction => !from.HasValue || transaction.Date >= from.Value)
            .Where(transaction => !to.HasValue || transaction.Date <= to.Value)
            .Where(transaction => scope switch
            {
                TransactionScope.Past => transaction.Date <= today,
                TransactionScope.Future => transaction.Date > today,
                _ => true
            })
            .OrderBy(transaction => transaction.Date)
            .ToList();
    }

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _transactionRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction.Id == Guid.Empty)
        {
            transaction.Id = Guid.NewGuid();
        }

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var existing = await _transactionRepository.GetByIdAsync(transaction.Id, cancellationToken);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Transaction '{transaction.Id}' was not found.");
        }

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Transaction '{id}' was not found.");
        }

        await _transactionRepository.DeleteAsync(id, cancellationToken);
    }
}