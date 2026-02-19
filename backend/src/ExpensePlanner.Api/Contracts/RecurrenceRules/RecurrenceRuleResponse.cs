using ExpensePlanner.Domain;

namespace ExpensePlanner.Api.Contracts.RecurrenceRules;

public sealed record RecurrenceRuleResponse(
    Guid Id,
    RecurrenceUnit Unit,
    int Interval,
    int DayIndex);