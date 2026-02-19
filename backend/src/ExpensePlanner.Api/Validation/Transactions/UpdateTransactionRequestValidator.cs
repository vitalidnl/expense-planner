using ExpensePlanner.Api.Contracts.Transactions;
using FluentValidation;

namespace ExpensePlanner.Api.Validation.Transactions;

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(request => request.Type)
            .IsInEnum();

        RuleFor(request => request.Amount)
            .GreaterThan(0m);

        RuleFor(request => request.Date)
            .NotEqual(default(DateOnly));

        RuleFor(request => request.Description)
            .MaximumLength(250)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));
    }
}