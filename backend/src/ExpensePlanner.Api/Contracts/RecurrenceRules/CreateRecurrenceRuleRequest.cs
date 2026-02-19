using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Contracts.RecurrenceRules;

public sealed record CreateRecurrenceRuleRequest(
    RecurrenceUnit Unit,
    int Interval,
    int DayIndex);