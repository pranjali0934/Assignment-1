using System.Net.Http.Json;
using System.Text.Json;
using ExpenseAssistant.Api.Config;
using ExpenseAssistant.Api.Models;
using Microsoft.Extensions.Options;

namespace ExpenseAssistant.Api.Services;

public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;
    private readonly string _systemInstruction;
    private readonly JsonElement _responseSchema;

    public GeminiClient(HttpClient http, IOptions<GeminiOptions> options, IHostEnvironment env)
    {
        _http = http;
        _options = options.Value;
        var (systemInstruction, responseSchemaJson) = ExtractionPromptLoader.Load(env.ContentRootPath);
        _systemInstruction = systemInstruction;
        _responseSchema = JsonDocument.Parse(responseSchemaJson).RootElement.Clone();
    }

    public async Task<RawExtractionResponse> ExtractAsync(string message, DateOnly requestDate, CancellationToken ct = default)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

        var requestBody = new
        {
            systemInstruction = new { parts = new[] { new { text = _systemInstruction } } },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = $"Current date: {requestDate:yyyy-MM-dd}. User message: {message}" } },
                },
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = _responseSchema,
                // Structured extraction doesn't need extended reasoning; disabling "thinking"
                // keeps latency within SC-005's <5s budget (a "thinking" response took ~18s
                // for this same task in live testing — see research.md/harness-log.md).
                thinkingConfig = new { thinkingBudget = 0 },
            },
        };

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(url, requestBody, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new GeminiApiException("Gemini API request failed (network error).", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new GeminiApiException("Gemini API request timed out.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new GeminiApiException($"Gemini API returned {(int)response.StatusCode}: {body}");
        }

        string rawText;
        try
        {
            using var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct)
                ?? throw new GeminiApiException("Gemini API returned an empty response body.");
            rawText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? throw new GeminiApiException("Gemini API response had no text content.");
        }
        catch (GeminiApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GeminiApiException("Unexpected Gemini API response shape.", ex);
        }

        try
        {
            return JsonSerializer.Deserialize<RawExtractionResponse>(
                rawText, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? new RawExtractionResponse(null, true, "The model returned an empty response.");
        }
        catch (JsonException)
        {
            // Structurally invalid JSON from the model itself (schema enforcement failed) —
            // ExtractionService's retry-then-fallback (T015) handles this, not a hard failure here.
            return new RawExtractionResponse(null, true, "The model's response could not be parsed.");
        }
    }
}
