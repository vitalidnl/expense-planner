using ExpensePlanner.Api.Contracts.RecurrenceRules;
using FluentValidation;

namespace ExpensePlanner.Api.Validation.RecurrenceRules;

public class CreateRecurrenceRuleRequestValidator : AbstractValidator<CreateRecurrenceRuleRequest>
{
    public CreateRecurrenceRuleRequestValidator()
    {
        RuleFor(request => request.Unit)
            .IsInEnum();

        RuleFor(request => request.Interval)
            .InclusiveBetween(1, 52);

        RuleFor(request => request)
            .Must(request => RecurrenceRuleValidationRules.IsValidDayIndex(request.Unit, request.DayIndex))
            .WithMessage("DayIndex is out of range for the specified Unit.");
    }
}