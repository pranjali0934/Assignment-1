using ExpenseAssistant.Api.Models;

namespace ExpenseAssistant.Api.Services;

public interface IGeminiClient
{
    /// <summary>
    /// Sends the user's message to Gemini for structured extraction, returning the raw
    /// (not-yet-validated) response. Throws GeminiApiException on any API/network failure, so
    /// callers can distinguish a model/service failure from an application-level one.
    /// </summary>
    Task<RawExtractionResponse> ExtractAsync(string message, DateOnly requestDate, CancellationToken ct = default);
}

public class GeminiApiException(string message, Exception? inner = null) : Exception(message, inner);
