using ExpensePlanner.Api.Contracts.RecurrenceRules;
using ExpensePlanner.Api.Contracts.RecurringTransactions;
using ExpensePlanner.Api.Contracts.Transactions;
using ExpensePlanner.Api.Validation.RecurrenceRules;
using ExpensePlanner.Api.Validation.RecurringTransactions;
using ExpensePlanner.Api.Validation.Transactions;
using ExpensePlanner.Domain;
using FluentValidation.TestHelper;

namespace ExpensePlanner.Api.Tests;

public class RequestValidatorsTests
{
    [Fact]
    public void CreateTransactionRequestValidator_InvalidAmount_HasValidationError()
    {
        var validator = new CreateTransactionRequestValidator();
        var request = new CreateTransactionRequest(TransactionType.Expense, 0m, new DateOnly(2025, 1, 1), null);

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(item => item.Amount);
    }

    [Fact]
    public void UpdateTransactionRequestValidator_ValidRequest_HasNoValidationErrors()
    {
        var validator = new UpdateTransactionRequestValidator();
        var request = new UpdateTransactionRequest(TransactionType.Income, 120m, new DateOnly(2025, 2, 1), "salary");

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateRecurrenceRuleRequestValidator_InvalidDayIndexForMonth_HasValidationError()
    {
        var validator = new CreateRecurrenceRuleRequestValidator();
        var request = new CreateRecurrenceRuleRequest(RecurrenceUnit.Month, 1, 31);

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(item => item);
    }

    [Fact]
    public void UpdateRecurrenceRuleRequestValidator_ValidYearlyRule_HasNoValidationErrors()
    {
        var validator = new UpdateRecurrenceRuleRequestValidator();
        var request = new UpdateRecurrenceRuleRequest(RecurrenceUnit.Year, 1, 366);

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateRecurringTransactionRequestValidator_EmptyRuleId_HasValidationError()
    {
        var validator = new CreateRecurringTransactionRequestValidator();
        var request = new CreateRecurringTransactionRequest(
            TransactionType.Expense,
            45m,
            new DateOnly(2025, 1, 10),
            Guid.Empty,
            "subscription",
            false);

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(item => item.RecurrenceRuleId);
    }

    [Fact]
    public void UpdateRecurringTransactionRequestValidator_ValidRequest_HasNoValidationErrors()
    {
        var validator = new UpdateRecurringTransactionRequestValidator();
        var request = new UpdateRecurringTransactionRequest(
            TransactionType.Income,
            900m,
            new DateOnly(2025, 1, 1),
            Guid.NewGuid(),
            "monthly income",
            false);

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}