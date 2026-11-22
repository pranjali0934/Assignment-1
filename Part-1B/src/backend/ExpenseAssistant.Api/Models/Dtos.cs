namespace ExpenseAssistant.Api.Models;

/// <summary>Incoming request from the client.</summary>
public record ExtractRequest(string Message);

/// <summary>
/// What the Gemini client deserializes the model's raw JSON into — deliberately loose-typed
/// (nullable/decimal/string) so ValidationService can detect and reject non-conforming values
/// (decimal amounts, unknown categories, unparseable dates) rather than having them silently
/// coerced by model binding.
/// </summary>
public record RawExtractedExpense(decimal? Amount, string? Category, string? Date, string? Description);

public record RawExtractionResponse(
    List<RawExtractedExpense>? Expenses,
    bool ClarificationNeeded,
    string? ClarificationQuestion);

/// <summary>The validated, API-facing response — only ever contains harness-safe values.</summary>
public record ExtractedExpenseResponse(int Amount, string Category, DateOnly Date, string? Description)
{
    public static ExtractedExpenseResponse FromEntity(ExtractedExpense e) =>
        new(e.Amount, e.Category.ToString(), e.Date, e.Description);
}

public record ExtractResponse(
    IReadOnlyList<ExtractedExpenseResponse> Expenses,
    bool ClarificationNeeded,
    string? ClarificationQuestion);

/// <summary>Internal result of ValidationService.Validate, before mapping to the API DTO.</summary>
public record ValidationResult(
    IReadOnlyList<ExtractedExpense> Expenses,
    bool ClarificationNeeded,
    string? ClarificationQuestion);
