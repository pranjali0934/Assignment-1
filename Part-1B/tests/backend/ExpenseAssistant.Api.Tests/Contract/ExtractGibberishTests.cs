using System.Net.Http.Json;
using ExpenseAssistant.Api.Models;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Contract;

public class ExtractGibberishTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExtractGibberishTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_GibberishInput_ReturnsClarificationWithNoFabricatedExpense()
    {
        _factory.Fake.Enqueue(new RawExtractionResponse(
            [], true, "I couldn't find an expense in your message. Could you describe what you spent?"));

        var response = await _client.PostAsJsonAsync("/api/extract", new ExtractRequest("asdf qwerty"));

        var body = await response.Content.ReadFromJsonAsync<ExtractResponse>();
        Assert.NotNull(body);
        Assert.Empty(body!.Expenses);
        Assert.True(body.ClarificationNeeded);
    }
}
