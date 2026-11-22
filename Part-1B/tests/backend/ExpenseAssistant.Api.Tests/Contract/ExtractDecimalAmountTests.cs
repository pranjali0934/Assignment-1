using System.Net;
using System.Net.Http.Json;
using ExpenseAssistant.Api.Models;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Contract;

public class ExtractDecimalAmountTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExtractDecimalAmountTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_ModelReturnsDecimalAmount_RejectsItAndRequestsClarification()
    {
        // Model (fake) returns a non-integer amount despite the prompt's instruction — the
        // harness must reject it independently, never silently round/truncate it.
        _factory.Fake.Enqueue(new RawExtractionResponse(
            [new RawExtractedExpense(500.50m, "Food", "2026-11-21", "lunch")],
            false,
            null));

        var response = await _client.PostAsJsonAsync(
            "/api/extract", new ExtractRequest("I spent 500.50 on lunch"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ExtractResponse>();
        Assert.NotNull(body);
        Assert.Empty(body!.Expenses);
        Assert.True(body.ClarificationNeeded);
    }
}
