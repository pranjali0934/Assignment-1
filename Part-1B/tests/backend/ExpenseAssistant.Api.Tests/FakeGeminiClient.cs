using ExpenseAssistant.Api.Models;
using ExpenseAssistant.Api.Services;

namespace ExpenseAssistant.Api.Tests;

/// <summary>
/// Test double for IGeminiClient — contract tests enqueue the exact raw response(s) they want
/// returned, so tests run deterministically and without a live Gemini API key (per research.md).
/// </summary>
public class FakeGeminiClient : IGeminiClient
{
    private readonly Queue<RawExtractionResponse> _responses = new();

    public void Enqueue(RawExtractionResponse response) => _responses.Enqueue(response);

    public Task<RawExtractionResponse> ExtractAsync(string message, DateOnly requestDate, CancellationToken ct = default)
    {
        var response = _responses.Count > 0
            ? _responses.Dequeue()
            : new RawExtractionResponse([], true, "No fake response was configured for this test.");
        return Task.FromResult(response);
    }
}
