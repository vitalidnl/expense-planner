using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Validation.RecurrenceRules;

internal static class RecurrenceRuleValidationRules
{
    public static bool IsValidDayIndex(RecurrenceUnit unit, int dayIndex)
    {
        return unit switch
        {
            RecurrenceUnit.Week => dayIndex is >= 1 and <= 7,
            RecurrenceUnit.Month => dayIndex is >= 1 and <= 28,
            RecurrenceUnit.Year => dayIndex is >= 1 and <= 366,
            _ => false
        };
    }
}