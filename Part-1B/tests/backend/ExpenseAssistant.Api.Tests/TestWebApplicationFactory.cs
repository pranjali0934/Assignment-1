using ExpenseAssistant.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExpenseAssistant.Api.Tests;

/// <summary>
/// Swaps the real HttpClient-backed GeminiClient for a FakeGeminiClient, so contract tests run
/// fast, deterministic, and without a live GEMINI_API_KEY (per research.md's testability
/// decision). Each factory instance gets its own FakeGeminiClient, exposed via
/// <see cref="Fake"/> so a test can enqueue the exact raw response it wants.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeGeminiClient Fake { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IGeminiClient>();
            services.AddSingleton<IGeminiClient>(Fake);
        });
    }
}
