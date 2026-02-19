using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Contracts.RecurrenceRules;

public sealed record UpdateRecurrenceRuleRequest(
    RecurrenceUnit Unit,
    int Interval,
    int DayIndex);