using ExpenseAssistant.Api.Models;

namespace ExpenseAssistant.Api.Services;

public class ExtractionService(IGeminiClient geminiClient, ValidationService validationService)
{
    public async Task<ExtractResponse> ExtractAsync(string message, CancellationToken ct = default)
    {
        var requestDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var raw = await geminiClient.ExtractAsync(message, requestDate, ct);
        var result = validationService.Validate(raw, requestDate);

        // Retry-then-fallback: if the first attempt produced nothing usable and no
        // clarification was flagged (an unexpected/empty shape from the model, not a case the
        // model itself identified as needing clarification), retry once before falling back to
        // a clarification response rather than ever surfacing an invalid/empty result silently.
        if (result.Expenses.Count == 0 && !result.ClarificationNeeded)
        {
            raw = await geminiClient.ExtractAsync(message, requestDate, ct);
            result = validationService.Validate(raw, requestDate);

            if (result.Expenses.Count == 0 && !result.ClarificationNeeded)
            {
                result = new ValidationResult(
                    [],
                    true,
                    "I couldn't understand that as an expense. Could you describe what you spent and how much?");
            }
        }

        return new ExtractResponse(
            [.. result.Expenses.Select(ExtractedExpenseResponse.FromEntity)],
            result.ClarificationNeeded,
            result.ClarificationQuestion);
    }
}
