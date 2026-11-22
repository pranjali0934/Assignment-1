using ExpenseAssistant.Api.Models;
using ExpenseAssistant.Api.Services;

namespace Evaluator;

/// <summary>
/// Layer 2 — invariant/property validation. Re-asserts the same rules ValidationService is
/// supposed to enforce (amount range, valid category enum) as a regression check at the
/// harness level, and checks the case-specific expected clarification outcome — independent of
/// whatever the model claimed (Constitution Principle III).
/// </summary>
public static class InvariantValidator
{
    public static (bool Pass, string? Reason) Check(ValidationResult validated, EvaluationCase testCase)
    {
        foreach (var expense in validated.Expenses)
        {
            if (expense.Amount < ValidationService.MinAmount || expense.Amount > ValidationService.MaxAmount)
            {
                return (false, $"Validated expense amount {expense.Amount} is out of the [1, 100000] range — ValidationService invariant was violated.");
            }

            if (!Enum.IsDefined(expense.Category))
            {
                return (false, $"Validated expense category '{expense.Category}' is not one of the fixed enum values.");
            }
        }

        if (testCase.Expected.ExpectClarification is { } expectClarification
            && validated.ClarificationNeeded != expectClarification)
        {
            return (false, $"Expected ClarificationNeeded={expectClarification} but got {validated.ClarificationNeeded}.");
        }

        if (testCase.Expected.ExpectedExpenseCount is { } expectedCount
            && validated.Expenses.Count != expectedCount)
        {
            return (false, $"Expected {expectedCount} expense(s) after validation but got {validated.Expenses.Count}.");
        }

        return (true, null);
    }
}
