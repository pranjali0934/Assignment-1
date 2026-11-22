using ExpenseAssistant.Api.Models;

namespace Evaluator;

/// <summary>
/// Layer 3 — semantic/quality evaluation. Scores the validated extraction against the case's
/// expected ground truth using an explicit rubric (exact amount/category match per expected
/// expense), not a bare LLM-as-Judge call, per Constitution Engineering Constraints.
/// </summary>
public static class SemanticEvaluator
{
    public static double Score(ValidationResult validated, EvaluationCase testCase)
    {
        var expectedExpenses = testCase.Expected.ExpectedExpenses;
        if (expectedExpenses is null || expectedExpenses.Count == 0)
        {
            // Nothing to score semantically beyond clarification correctness, which
            // InvariantValidator already checked for this case.
            return 1.0;
        }

        var remaining = new List<ExtractedExpense>(validated.Expenses);
        var matched = 0;

        foreach (var expected in expectedExpenses)
        {
            var match = remaining.FirstOrDefault(e =>
                (expected.Amount is null || e.Amount == expected.Amount) &&
                (expected.Category is null
                    || string.Equals(e.Category.ToString(), expected.Category, StringComparison.OrdinalIgnoreCase)));

            if (match is null)
            {
                continue;
            }

            matched++;
            remaining.Remove(match);
        }

        return (double)matched / expectedExpenses.Count;
    }
}
