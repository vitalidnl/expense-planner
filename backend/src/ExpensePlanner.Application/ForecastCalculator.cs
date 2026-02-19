using ExpensePlanner.Domain;

namespace ExpensePlanner.Application;

public sealed record DailyBalancePoint(
    DateOnly Date,
    decimal DailyNet,
    decimal Balance);

public sealed record ForecastResult(
    IReadOnlyList<DailyBalancePoint> DailyBalances);

/// <summary>
/// Calculates forecast balances by aggregating one-time and recurring transactions at day granularity.
/// </summary>
public class ForecastCalculator
{
    private readonly RecurrenceOccurrenceGenerator _occurrenceGenerator;

    public ForecastCalculator(RecurrenceOccurrenceGenerator? occurrenceGenerator = null)
    {
        _occurrenceGenerator = occurrenceGenerator ?? new RecurrenceOccurrenceGenerator();
    }

    public ForecastResult CalculateDailyBalances(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<RecurringTransaction> recurringTransactions,
        IReadOnlyCollection<RecurrenceRule> recurrenceRules,
        DateOnly from,
        DateOnly to)
    {
        if (from > to)
        {
            return new ForecastResult([]);
        }

        var ruleById = recurrenceRules.ToDictionary(rule => rule.Id);
        var dailyNets = new Dictionary<DateOnly, decimal>();

        foreach (var transaction in transactions)
        {
            if (transaction.Date > to)
            {
                continue;
            }

            AddDailyNet(dailyNets, transaction.Date, SignedAmount(transaction.Type, transaction.Amount));
        }

        foreach (var recurringTransaction in recurringTransactions)
        {
            if (!ruleById.TryGetValue(recurringTransaction.RecurrenceRuleId, out var rule))
            {
                throw new InvalidOperationException(
                    $"Recurrence rule '{recurringTransaction.RecurrenceRuleId}' was not found for recurring transaction '{recurringTransaction.Id}'.");
            }

            var occurrences = _occurrenceGenerator.Generate(
                recurringTransaction,
                rule,
                recurringTransaction.StartDate,
                to);

            foreach (var occurrenceDate in occurrences)
            {
                AddDailyNet(dailyNets, occurrenceDate, SignedAmount(recurringTransaction.Type, recurringTransaction.Amount));
            }
        }

        var balanceBeforeRange = dailyNets
            .Where(entry => entry.Key < from)
            .Sum(entry => entry.Value);

        var balance = balanceBeforeRange;
        var points = new List<DailyBalancePoint>();

        var currentDate = from;
        while (currentDate <= to)
        {
            var dailyNet = dailyNets.GetValueOrDefault(currentDate, 0m);
            balance += dailyNet;
            points.Add(new DailyBalancePoint(currentDate, dailyNet, balance));
            currentDate = currentDate.AddDays(1);
        }

        return new ForecastResult(points);
    }

    public decimal CalculateBalanceAtDate(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<RecurringTransaction> recurringTransactions,
        IReadOnlyCollection<RecurrenceRule> recurrenceRules,
        DateOnly date)
    {
        var forecast = CalculateDailyBalances(transactions, recurringTransactions, recurrenceRules, date, date);
        return forecast.DailyBalances.Count == 0
            ? 0m
            : forecast.DailyBalances[0].Balance;
    }

    private static decimal SignedAmount(TransactionType transactionType, decimal amount) =>
        transactionType == TransactionType.Income ? amount : -amount;

    private static void AddDailyNet(IDictionary<DateOnly, decimal> dailyNets, DateOnly date, decimal signedAmount)
    {
        if (dailyNets.TryGetValue(date, out var existing))
        {
            dailyNets[date] = existing + signedAmount;
            return;
        }

        dailyNets[date] = signedAmount;
    }
}