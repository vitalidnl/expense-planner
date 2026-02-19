namespace ExpensePlanner.Application;

public class ForecastService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;
    private readonly IRecurrenceRuleRepository _recurrenceRuleRepository;
    private readonly ForecastCalculator _forecastCalculator;

    public ForecastService(
        ITransactionRepository transactionRepository,
        IRecurringTransactionRepository recurringTransactionRepository,
        IRecurrenceRuleRepository recurrenceRuleRepository,
        ForecastCalculator? forecastCalculator = null)
    {
        _transactionRepository = transactionRepository;
        _recurringTransactionRepository = recurringTransactionRepository;
        _recurrenceRuleRepository = recurrenceRuleRepository;
        _forecastCalculator = forecastCalculator ?? new ForecastCalculator();
    }

    public async Task<ForecastResult> GetForecastAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetAllAsync(cancellationToken);
        var recurringTransactions = await _recurringTransactionRepository.GetAllAsync(cancellationToken);
        var recurrenceRules = await _recurrenceRuleRepository.GetAllAsync(cancellationToken);

        return _forecastCalculator.CalculateDailyBalances(transactions, recurringTransactions, recurrenceRules, from, to);
    }

    public async Task<decimal> GetBalanceAtDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetAllAsync(cancellationToken);
        var recurringTransactions = await _recurringTransactionRepository.GetAllAsync(cancellationToken);
        var recurrenceRules = await _recurrenceRuleRepository.GetAllAsync(cancellationToken);

        return _forecastCalculator.CalculateBalanceAtDate(transactions, recurringTransactions, recurrenceRules, date);
    }
}