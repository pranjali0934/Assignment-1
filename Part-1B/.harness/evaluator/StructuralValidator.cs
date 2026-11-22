using ExpenseAssistant.Api.Models;

namespace Evaluator;

/// <summary>
/// Layer 1 — structural validation of the model's raw output: did Gemini return a parseable,
/// minimally well-shaped response at all (independent of whether the VALUES within it are
/// correct — that's Layer 2/3). A parse failure in GeminiClient already degrades to a
/// clarification fallback (see GeminiClient.cs), which is itself a structurally valid shape,
/// so this mainly catches shape issues that survive deserialization (e.g. a null Category on
/// an expense that claims an amount).
/// </summary>
public static class StructuralValidator
{
    public static bool IsStructurallyValid(RawExtractionResponse raw, out string? reason)
    {
        if (raw.Expenses is null)
        {
            reason = "Expenses field was null (expected at least an empty array).";
            return false;
        }

        foreach (var expense in raw.Expenses)
        {
            if (expense.Amount is not null && string.IsNullOrWhiteSpace(expense.Category))
            {
                reason = "An expense with a stated amount had no category.";
                return false;
            }
        }

        if (raw.ClarificationNeeded && string.IsNullOrWhiteSpace(raw.ClarificationQuestion))
        {
            reason = "ClarificationNeeded was true but ClarificationQuestion was empty.";
            return false;
        }

        reason = null;
        return true;
    }
}
