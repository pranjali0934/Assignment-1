using System.Diagnostics;
using System.Text.Json;
using Evaluator;
using ExpenseAssistant.Api.Config;
using ExpenseAssistant.Api.Models;
using ExpenseAssistant.Api.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var datasetPath = GetArg(args, "--dataset") ?? "../../evals/dataset.json";
var outPath = GetArg(args, "--out") ?? "../../evidence/eval-results.json";

var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("ERROR: GEMINI_API_KEY environment variable is not set.");
    return 1;
}

var geminiOptions = Options.Create(new GeminiOptions { ApiKey = apiKey });
using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
// ExtractionPromptLoader resolves the prompt file as "<contentRoot>/../../prompts/...", a path
// shape designed around the API project's own location (src/backend/ExpenseAssistant.Api).
// Point the evaluator's "content root" at that same real project directory, rather than its
// own build output folder (AppContext.BaseDirectory), so the one prompt file stays the single
// source of truth for both the API and this harness.
var apiProjectContentRoot = Path.GetFullPath(
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "backend", "ExpenseAssistant.Api"));
var geminiClient = new GeminiClient(httpClient, geminiOptions, new MinimalHostEnvironment(apiProjectContentRoot));
var validationService = new ValidationService();

var casesJson = await File.ReadAllTextAsync(datasetPath);
var cases = JsonSerializer.Deserialize<List<EvaluationCase>>(casesJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))
    ?? throw new InvalidOperationException($"Could not parse any cases from {datasetPath}");

Console.WriteLine($"Running {cases.Count} evaluation case(s) against the live Gemini API...");

var results = new List<EvaluationResult>();
foreach (var testCase in cases)
{
    var requestDate = DateOnly.FromDateTime(DateTime.UtcNow);
    var stopwatch = Stopwatch.StartNew();

    RawExtractionResponse raw;
    try
    {
        raw = await geminiClient.ExtractAsync(testCase.Input, requestDate);
    }
    catch (GeminiApiException ex)
    {
        stopwatch.Stop();
        results.Add(new EvaluationResult(
            testCase.Id, false, false, 0, false, stopwatch.ElapsedMilliseconds, "",
            $"Gemini API/service failure (not an evaluation failure): {ex.Message}"));
        Console.WriteLine($"  [{testCase.Id}] SERVICE FAILURE: {ex.Message}");
        continue;
    }

    stopwatch.Stop();
    var rawJson = JsonSerializer.Serialize(raw);

    var structuralPass = StructuralValidator.IsStructurallyValid(raw, out var structuralReason);
    var validated = validationService.Validate(raw, requestDate);
    var (invariantPass, invariantReason) = InvariantValidator.Check(validated, testCase);
    var semanticScore = SemanticEvaluator.Score(validated, testCase);

    var requiresExactSemanticMatch = testCase.Expected.ExpectedExpenses is { Count: > 0 };
    var overallPass = structuralPass && invariantPass && (!requiresExactSemanticMatch || semanticScore >= 0.999);

    var failureReason = overallPass
        ? null
        : structuralReason ?? invariantReason ?? $"Semantic score {semanticScore:0.00} below required exact match.";

    results.Add(new EvaluationResult(
        testCase.Id, structuralPass, invariantPass, semanticScore, overallPass,
        stopwatch.ElapsedMilliseconds, rawJson, failureReason));

    Console.WriteLine($"  [{testCase.Id}] {(overallPass ? "PASS" : "FAIL")} ({stopwatch.ElapsedMilliseconds}ms)" +
        (overallPass ? "" : $" — {failureReason}"));
}

var evaluatedResults = results.Where(r => r.FailureReason is null || !r.FailureReason.StartsWith("Gemini API/service failure")).ToList();
var serviceFailures = results.Count - evaluatedResults.Count;
var passCount = results.Count(r => r.OverallPass);
var passRate = results.Count == 0 ? 0 : (double)passCount / results.Count;
const double threshold = 0.85;
var aggregatePass = passRate >= threshold;

var latencies = results.Where(r => r.LatencyMs > 0).Select(r => r.LatencyMs).ToList();
var avgLatencyMs = latencies.Count == 0 ? 0 : latencies.Average();
var maxLatencyMs = latencies.Count == 0 ? 0 : latencies.Max();
const long sc005ThresholdMs = 5000;
var sc005Pass = maxLatencyMs < sc005ThresholdMs;

var summary = new
{
    totalCases = results.Count,
    passCount,
    passRate,
    threshold,
    aggregatePass,
    serviceFailures,
    avgLatencyMs,
    maxLatencyMs,
    sc005ThresholdMs,
    sc005Pass,
    results,
};

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine();
Console.WriteLine($"Pass rate: {passCount}/{results.Count} ({passRate:P0}) — threshold {threshold:P0} — {(aggregatePass ? "AGGREGATE PASS" : "AGGREGATE FAIL")}");
Console.WriteLine($"Latency: avg {avgLatencyMs:0}ms, max {maxLatencyMs}ms — SC-005 (<{sc005ThresholdMs}ms) {(sc005Pass ? "PASS" : "FAIL")}");
Console.WriteLine($"Service failures (excluded from pass-rate denominator consideration): {serviceFailures}");
Console.WriteLine($"Results written to {Path.GetFullPath(outPath)}");

return aggregatePass && sc005Pass ? 0 : 1;

static string? GetArg(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

/// <summary>Minimal IHostEnvironment so GeminiClient (built for DI) can run outside ASP.NET Core's host.</summary>
file class MinimalHostEnvironment(string contentRootPath) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Evaluation";
    public string ApplicationName { get; set; } = "Evaluator";
    public string ContentRootPath { get; set; } = contentRootPath;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
