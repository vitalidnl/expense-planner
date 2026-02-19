using ExpensePlanner.Api.Contracts.RecurringTransactions;
using FluentValidation;

namespace ExpensePlanner.Api.Validation.RecurringTransactions;

public class UpdateRecurringTransactionRequestValidator : AbstractValidator<UpdateRecurringTransactionRequest>
{
    public UpdateRecurringTransactionRequestValidator()
    {
        RuleFor(request => request.Type)
            .IsInEnum();

        RuleFor(request => request.Amount)
            .GreaterThan(0m);

        RuleFor(request => request.StartDate)
            .NotEqual(default(DateOnly));

        RuleFor(request => request.RecurrenceRuleId)
            .NotEqual(Guid.Empty);

        RuleFor(request => request.Description)
            .MaximumLength(250)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));
    }
}