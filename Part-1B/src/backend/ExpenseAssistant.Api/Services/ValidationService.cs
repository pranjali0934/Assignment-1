using ExpenseAssistant.Api.Models;

namespace ExpenseAssistant.Api.Services;

/// <summary>
/// Enforces the constitution's amount/category/no-fabrication/date rules on the model's raw
/// output — independent of whatever the model itself claims (Constitution Principle III).
/// This is the harness's Layer 2 (invariant) logic, reused as-is by the evaluation harness in
/// .harness/evaluator/InvariantValidator.cs.
/// </summary>
public class ValidationService
{
    public const int MinAmount = 1;
    public const int MaxAmount = 100_000;

    public ValidationResult Validate(RawExtractionResponse raw, DateOnly requestDate)
    {
        var validExpenses = new List<ExtractedExpense>();
        var clarificationNeeded = raw.ClarificationNeeded;
        var question = raw.ClarificationQuestion;

        foreach (var rawExpense in raw.Expenses ?? [])
        {
            if (!TryValidateAmount(rawExpense.Amount, out var amount))
            {
                clarificationNeeded = true;
                question ??= "What amount did you spend? I couldn't find a valid amount in your message.";
                continue; // Drop this expense rather than fabricate an amount (FR-006/FR-009).
            }

            var category = MapCategory(rawExpense.Category);

            // T012: deterministic date defaulting — independent of whether the model honored
            // the prompt's date-handling instruction. Never left to the model alone.
            var date = TryParseDate(rawExpense.Date, out var parsedDate) ? parsedDate : requestDate;

            validExpenses.Add(new ExtractedExpense
            {
                Amount = amount,
                Category = category,
                Date = date,
                Description = rawExpense.Description,
            });
        }

        if (clarificationNeeded && string.IsNullOrWhiteSpace(question))
        {
            question = "Could you clarify the amount for this expense?";
        }

        return new ValidationResult(validExpenses, clarificationNeeded, clarificationNeeded ? question : null);
    }

    /// <summary>
    /// Rejects null, out-of-range, and non-integer (decimal) amounts — the amount is received
    /// as a decimal specifically so a model-returned value like 500.50 is detectable here,
    /// rather than being silently rounded/truncated by earlier deserialization.
    /// </summary>
    private static bool TryValidateAmount(decimal? rawAmount, out int amount)
    {
        amount = 0;
        if (rawAmount is not { } value)
        {
            return false;
        }

        if (value != Math.Truncate(value))
        {
            return false; // Decimal amount — reject, never round/truncate (constitution rule).
        }

        if (value < MinAmount || value > MaxAmount)
        {
            return false;
        }

        amount = (int)value;
        return true;
    }

    private static Category MapCategory(string? rawCategory)
    {
        if (!string.IsNullOrWhiteSpace(rawCategory) && Enum.TryParse<Category>(rawCategory, ignoreCase: true, out var category))
        {
            return category;
        }

        // Unknown/unmapped category values fall back to "Other" rather than being rejected
        // outright (spec Edge Cases) — the category list is fixed, but a sensible catch-all
        // keeps a partially-correct extraction usable.
        return Category.Other;
    }

    private static bool TryParseDate(string? rawDate, out DateOnly date)
    {
        if (!string.IsNullOrWhiteSpace(rawDate) && DateOnly.TryParse(rawDate, out date))
        {
            return true;
        }

        date = default;
        return false;
    }
}
