using ExpensePlanner.Domain;

namespace ExpensePlanner.Application;

/// <summary>
/// Generates occurrence dates for a <see cref="RecurringTransaction"/> within a given date range,
/// based on its associated <see cref="RecurrenceRule"/>.
/// </summary>
public class RecurrenceOccurrenceGenerator
{
    /// <summary>
    /// Generates all occurrence dates within [<paramref name="from"/>, <paramref name="to"/>] inclusive.
    /// </summary>
    /// <param name="recurringTransaction">The recurring transaction template.</param>
    /// <param name="rule">The recurrence rule driving the pattern.</param>
    /// <param name="from">The start of the requested date range (inclusive).</param>
    /// <param name="to">The end of the requested date range (inclusive).</param>
    /// <returns>
    /// An ordered list of dates on which the transaction occurs within the range.
    /// Returns an empty list when the transaction is paused or no occurrences fall in the range.
    /// </returns>
    public IReadOnlyList<DateOnly> Generate(
        RecurringTransaction recurringTransaction,
        RecurrenceRule rule,
        DateOnly from,
        DateOnly to)
    {
        if (recurringTransaction.IsPaused)
            return [];

        // Occurrences cannot start before the recurring transaction's own start date.
        var effectiveFrom = recurringTransaction.StartDate > from
            ? recurringTransaction.StartDate
            : from;

        if (effectiveFrom > to)
            return [];

        return rule.Unit switch
        {
            RecurrenceUnit.Week  => GenerateWeekly(rule,  recurringTransaction.StartDate, effectiveFrom, to),
            RecurrenceUnit.Month => GenerateMonthly(rule, recurringTransaction.StartDate, effectiveFrom, to),
            RecurrenceUnit.Year  => GenerateYearly(rule,  recurringTransaction.StartDate, effectiveFrom, to),
            _ => throw new ArgumentOutOfRangeException(nameof(rule.Unit), rule.Unit, "Unknown recurrence unit.")
        };
    }

    // -------------------------------------------------------------------------
    // Weekly
    // -------------------------------------------------------------------------

    private static IReadOnlyList<DateOnly> GenerateWeekly(
        RecurrenceRule rule, DateOnly startDate, DateOnly from, DateOnly to)
    {
        // DayIndex uses 1=Monday … 7=Sunday; .NET DayOfWeek uses Sunday=0, Monday=1 … Saturday=6.
        var targetDay = rule.DayIndex == 7 ? DayOfWeek.Sunday : (DayOfWeek)rule.DayIndex;

        // Find the first occurrence on or after startDate that lands on the target weekday.
        var anchor = startDate;
        while (anchor.DayOfWeek != targetDay)
            anchor = anchor.AddDays(1);

        // Advance anchor by whole intervals to reach the first occurrence >= from.
        if (anchor < from)
        {
            var intervalDays = rule.Interval * 7;
            var steps = (int)Math.Ceiling((from.DayNumber - anchor.DayNumber) / (double)intervalDays);
            anchor = anchor.AddDays(steps * intervalDays);
        }

        var results = new List<DateOnly>();
        var current = anchor;

        while (current <= to)
        {
            results.Add(current);
            current = current.AddDays(rule.Interval * 7);
        }

        return results;
    }

    // -------------------------------------------------------------------------
    // Monthly
    // -------------------------------------------------------------------------

    private static IReadOnlyList<DateOnly> GenerateMonthly(
        RecurrenceRule rule, DateOnly startDate, DateOnly from, DateOnly to)
    {
        // DayIndex is 1..28, guaranteed to exist in every month — no clamping needed.
        var anchor = new DateOnly(startDate.Year, startDate.Month, rule.DayIndex);

        // If the occurrence in startDate's month is before startDate, advance one interval.
        if (anchor < startDate)
            anchor = anchor.AddMonths(rule.Interval);

        // Advance to first occurrence >= from.
        while (anchor < from)
            anchor = anchor.AddMonths(rule.Interval);

        var results = new List<DateOnly>();
        var current = anchor;

        while (current <= to)
        {
            results.Add(current);
            current = current.AddMonths(rule.Interval);
        }

        return results;
    }

    // -------------------------------------------------------------------------
    // Yearly
    // -------------------------------------------------------------------------

    private static IReadOnlyList<DateOnly> GenerateYearly(
        RecurrenceRule rule, DateOnly startDate, DateOnly from, DateOnly to)
    {
        // Find the first valid occurrence on or after startDate.
        var anchor = FindFirstYearlyAnchor(rule.DayIndex, startDate);

        if (anchor > to)
            return [];

        // Advance to first occurrence >= from.
        var current = anchor;
        while (current < from)
            current = NextYearlyOccurrence(current.Year, rule.Interval, rule.DayIndex);

        var results = new List<DateOnly>();

        while (current <= to)
        {
            results.Add(current);
            current = NextYearlyOccurrence(current.Year, rule.Interval, rule.DayIndex);
        }

        return results;
    }

    /// <summary>
    /// Finds the first valid occurrence of <paramref name="dayIndex"/> on or after <paramref name="startDate"/>.
    /// When <paramref name="dayIndex"/> is 366 and a candidate year is not a leap year, that year is skipped.
    /// </summary>
    private static DateOnly FindFirstYearlyAnchor(int dayIndex, DateOnly startDate)
    {
        var year = startDate.Year;

        while (true)
        {
            var candidate = DayOfYearToDate(year, dayIndex);
            if (candidate is not null && candidate >= startDate)
                return candidate.Value;

            year++;
        }
    }

    /// <summary>
    /// Returns the next yearly occurrence after <paramref name="currentYear"/>, advancing by
    /// <paramref name="intervalYears"/>. Skips non-leap years when <paramref name="dayIndex"/> is 366.
    /// </summary>
    private static DateOnly NextYearlyOccurrence(int currentYear, int intervalYears, int dayIndex)
    {
        var nextYear = currentYear + intervalYears;

        while (true)
        {
            var candidate = DayOfYearToDate(nextYear, dayIndex);
            if (candidate is not null)
                return candidate.Value;

            nextYear += intervalYears;
        }
    }

    /// <summary>
    /// Converts a 1-based day-of-year index to a <see cref="DateOnly"/> for the given year.
    /// Returns <see langword="null"/> when <paramref name="dayIndex"/> is 366 and the year is not a leap year.
    /// </summary>
    private static DateOnly? DayOfYearToDate(int year, int dayIndex)
    {
        if (dayIndex == 366 && !DateTime.IsLeapYear(year))
            return null;

        return new DateOnly(year, 1, 1).AddDays(dayIndex - 1);
    }
}
