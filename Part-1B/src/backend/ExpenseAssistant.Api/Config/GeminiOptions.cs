namespace ExpenseAssistant.Api.Config;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>Read from the GEMINI_API_KEY environment variable or User Secrets — never committed.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Flash tier by default — fast/cheap, sufficient for structured extraction (see research.md).
    /// gemini-3.8-flash (Google's current recommendation) proved unstable (intermittent 503s
    /// and connection hangs) when live-tested; gemini-3.5-flash responded reliably, so that is
    /// used instead — documented as a known model-selection tradeoff in harness-log.md.
    /// </summary>
    public string Model { get; set; } = "gemini-3.5-flash";
}
