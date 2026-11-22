using System.Net.Http.Json;
using ExpenseAssistant.Api.Models;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Contract;

public class ExtractClarificationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExtractClarificationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_NoAmountDeterminable_ReturnsClarificationWithEmptyExpenses()
    {
        _factory.Fake.Enqueue(new RawExtractionResponse(
            [], true, "What amount did you spend? I couldn't find one in your message."));

        var response = await _client.PostAsJsonAsync(
            "/api/extract", new ExtractRequest("I had dinner with friends"));

        var body = await response.Content.ReadFromJsonAsync<ExtractResponse>();
        Assert.NotNull(body);
        Assert.Empty(body!.Expenses);
        Assert.True(body.ClarificationNeeded);
        Assert.False(string.IsNullOrWhiteSpace(body.ClarificationQuestion));
    }
}
