using ExpensePlanner.Application;
using ExpensePlanner.Domain;

namespace ExpensePlanner.Application.Tests;

public class RecurrenceOccurrenceGeneratorTests
{
    private readonly RecurrenceOccurrenceGenerator _generator = new();

    // =========================================================================
    // Helpers
    // =========================================================================

    private static RecurringTransaction MakeRecurring(
        DateOnly startDate,
        Guid ruleId,
        bool isPaused = false) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Amount = 100m,
            StartDate = startDate,
            RecurrenceRuleId = ruleId,
            IsPaused = isPaused
        };

    private static RecurrenceRule MakeRule(RecurrenceUnit unit, int interval, int dayIndex) =>
        new()
        {
            Id = Guid.NewGuid(),
            Unit = unit,
            Interval = interval,
            DayIndex = dayIndex
        };

    // =========================================================================
    // Paused / empty-range guards
    // =========================================================================

    [Fact]
    public void Generate_WhenPaused_ReturnsEmpty()
    {
        var rule = MakeRule(RecurrenceUnit.Month, 1, 15);
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id, isPaused: true);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

        Assert.Empty(result);
    }

    [Fact]
    public void Generate_WhenFromIsAfterTo_ReturnsEmpty()
    {
        var rule = MakeRule(RecurrenceUnit.Month, 1, 15);
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 6, 1), new DateOnly(2025, 5, 1));

        Assert.Empty(result);
    }

    [Fact]
    public void Generate_WhenStartDateIsAfterTo_ReturnsEmpty()
    {
        var rule = MakeRule(RecurrenceUnit.Month, 1, 15);
        var rt   = MakeRecurring(new DateOnly(2026, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

        Assert.Empty(result);
    }

    // =========================================================================
    // Weekly
    // =========================================================================

    [Fact]
    public void Generate_Weekly_EveryWeek_Monday_ReturnsCorrectDates()
    {
        // Jan 1 2025 = Wednesday; first Monday >= Jan 1 is Jan 6.
        var rule = MakeRule(RecurrenceUnit.Week, 1, dayIndex: 1); // Monday
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        Assert.Equal(
        [
            new DateOnly(2025, 1,  6),
            new DateOnly(2025, 1, 13),
            new DateOnly(2025, 1, 20),
            new DateOnly(2025, 1, 27)
        ], result);
    }

    [Fact]
    public void Generate_Weekly_EveryTwoWeeks_Friday_ReturnsCorrectDates()
    {
        // Jan 1 2025 (Wed) → first Friday is Jan 3; bi-weekly from there.
        var rule = MakeRule(RecurrenceUnit.Week, 2, dayIndex: 5); // Friday
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 28));

        Assert.Equal(
        [
            new DateOnly(2025, 1,  3),
            new DateOnly(2025, 1, 17),
            new DateOnly(2025, 1, 31),
            new DateOnly(2025, 2, 14),
            new DateOnly(2025, 2, 28)
        ], result);
    }

    [Fact]
    public void Generate_Weekly_DayIndex7_TreatedAsSunday()
    {
        // Jan 1 2025 (Wed) → first Sunday is Jan 5.
        var rule = MakeRule(RecurrenceUnit.Week, 1, dayIndex: 7); // Sunday
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        Assert.Equal(
        [
            new DateOnly(2025, 1,  5),
            new DateOnly(2025, 1, 12),
            new DateOnly(2025, 1, 19),
            new DateOnly(2025, 1, 26)
        ], result);
    }

    [Fact]
    public void Generate_Weekly_StartDateAlreadyOnTargetDay_IncludesStartDate()
    {
        // Jan 1 2025 is Wednesday; DayIndex=3 maps to Wednesday.
        var rule = MakeRule(RecurrenceUnit.Week, 1, dayIndex: 3); // Wednesday
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 15));

        Assert.Equal(
        [
            new DateOnly(2025, 1,  1),
            new DateOnly(2025, 1,  8),
            new DateOnly(2025, 1, 15)
        ], result);
    }

    [Fact]
    public void Generate_Weekly_RangeStartsAfterStartDate_SkipsEarlierOccurrences()
    {
        // Pattern: every Monday; StartDate = Jan 1 2025.
        // from = Jan 20 2025 → should only return Jan 20 and Jan 27.
        var rule = MakeRule(RecurrenceUnit.Week, 1, dayIndex: 1); // Monday
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 20), new DateOnly(2025, 1, 31));

        Assert.Equal(
        [
            new DateOnly(2025, 1, 20),
            new DateOnly(2025, 1, 27)
        ], result);
    }

    // =========================================================================
    // Monthly
    // =========================================================================

    [Fact]
    public void Generate_Monthly_EveryMonth_Day15_ReturnsCorrectDates()
    {
        var rule = MakeRule(RecurrenceUnit.Month, 1, dayIndex: 15);
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 6, 30));

        Assert.Equal(
        [
            new DateOnly(2025, 1, 15),
            new DateOnly(2025, 2, 15),
            new DateOnly(2025, 3, 15),
            new DateOnly(2025, 4, 15),
            new DateOnly(2025, 5, 15),
            new DateOnly(2025, 6, 15)
        ], result);
    }

    [Fact]
    public void Generate_Monthly_WhenDayIndexBeforeStartDay_AnchorAdvancedByInterval()
    {
        // StartDate March 10; DayIndex=5 → March 5 is before March 10, so anchor = May 5 (interval=2).
        var rule = MakeRule(RecurrenceUnit.Month, 2, dayIndex: 5);
        var rt   = MakeRecurring(new DateOnly(2025, 3, 10), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

        Assert.Equal(
        [
            new DateOnly(2025,  5, 5),
            new DateOnly(2025,  7, 5),
            new DateOnly(2025,  9, 5),
            new DateOnly(2025, 11, 5)
        ], result);
    }

    [Fact]
    public void Generate_Monthly_EveryTwoMonths_Day28_ReturnsCorrectDates()
    {
        // Day 28 exists in every month; interval = 2.
        var rule = MakeRule(RecurrenceUnit.Month, 2, dayIndex: 28);
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2025, 8, 31));

        Assert.Equal(
        [
            new DateOnly(2025, 1, 28),
            new DateOnly(2025, 3, 28),
            new DateOnly(2025, 5, 28),
            new DateOnly(2025, 7, 28)
        ], result);
    }

    [Fact]
    public void Generate_Monthly_RangeStartsAfterStartDate_SkipsEarlierOccurrences()
    {
        var rule = MakeRule(RecurrenceUnit.Month, 1, dayIndex: 1);
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        // from = April 1 → should only return Apr-Jun occurrences.
        var result = _generator.Generate(rt, rule, new DateOnly(2025, 4, 1), new DateOnly(2025, 6, 30));

        Assert.Equal(
        [
            new DateOnly(2025, 4, 1),
            new DateOnly(2025, 5, 1),
            new DateOnly(2025, 6, 1)
        ], result);
    }

    // =========================================================================
    // Yearly
    // =========================================================================

    [Fact]
    public void Generate_Yearly_EveryYear_Day1_ReturnsJan1EachYear()
    {
        var rule = MakeRule(RecurrenceUnit.Year, 1, dayIndex: 1); // January 1
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2027, 12, 31));

        Assert.Equal(
        [
            new DateOnly(2025, 1, 1),
            new DateOnly(2026, 1, 1),
            new DateOnly(2027, 1, 1)
        ], result);
    }

    [Fact]
    public void Generate_Yearly_DayIndex366_InLeapYear_IncludesOccurrence()
    {
        // 2024 is a leap year; day 366 = Dec 31.
        var rule = MakeRule(RecurrenceUnit.Year, 4, dayIndex: 366);
        var rt   = MakeRecurring(new DateOnly(2024, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2024, 1, 1), new DateOnly(2030, 12, 31));

        // 2024 leap → Dec 31; 2028 leap → Dec 31; 2032 is outside range.
        Assert.Equal(
        [
            new DateOnly(2024, 12, 31),
            new DateOnly(2028, 12, 31)
        ], result);
    }

    [Fact]
    public void Generate_Yearly_DayIndex366_SkipsNonLeapYears()
    {
        // StartDate in 2025 (not a leap year) → first occurrence is 2028.
        var rule = MakeRule(RecurrenceUnit.Year, 1, dayIndex: 366);
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        // Expect occurrences only in leap years: 2028, 2032, ... within range.
        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2033, 12, 31));

        Assert.Equal(
        [
            new DateOnly(2028, 12, 31),
            new DateOnly(2032, 12, 31)
        ], result);
    }

    [Fact]
    public void Generate_Yearly_WhenDayIndexOccurredBeforeStartdate_AnchorMovesToNextYear()
    {
        // Day 91 = Apr 1; StartDate = Jun 1 2025 → first occurrence in 2025 (Apr 1) is before June, so advance to 2026.
        var rule = MakeRule(RecurrenceUnit.Year, 1, dayIndex: 91); // Apr 1 in non-leap year
        var rt   = MakeRecurring(new DateOnly(2025, 6, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2027, 12, 31));

        Assert.Equal(
        [
            new DateOnly(2026, 4, 1),
            new DateOnly(2027, 4, 1)
        ], result);
    }

    [Fact]
    public void Generate_Yearly_EveryTwoYears_ReturnsCorrectDates()
    {
        var rule = MakeRule(RecurrenceUnit.Year, 2, dayIndex: 1); // Jan 1, every 2 years
        var rt   = MakeRecurring(new DateOnly(2025, 1, 1), rule.Id);

        var result = _generator.Generate(rt, rule, new DateOnly(2025, 1, 1), new DateOnly(2031, 12, 31));

        Assert.Equal(
        [
            new DateOnly(2025, 1, 1),
            new DateOnly(2027, 1, 1),
            new DateOnly(2029, 1, 1),
            new DateOnly(2031, 1, 1)
        ], result);
    }
}
