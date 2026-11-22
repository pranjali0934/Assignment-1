using System.Net.Http.Json;
using ExpenseAssistant.Api.Models;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Contract;

public class ExtractPartialMultipleExpensesTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExtractPartialMultipleExpensesTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_OneWellFormedAndOneMissingAmount_ReturnsPartialSuccess()
    {
        // The fake model itself returns ClarificationNeeded: true alongside one valid expense —
        // mirroring what a real model would do for "₹500 on groceries and something on petrol".
        _factory.Fake.Enqueue(new RawExtractionResponse(
            [new RawExtractedExpense(500, "Food", "2026-11-22", "groceries")],
            true,
            "What amount did you spend on petrol?"));

        var response = await _client.PostAsJsonAsync(
            "/api/extract", new ExtractRequest("I spent ₹500 on groceries and something on petrol"));

        var body = await response.Content.ReadFromJsonAsync<ExtractResponse>();
        Assert.NotNull(body);
        var expense = Assert.Single(body!.Expenses);
        Assert.Equal(500, expense.Amount);
        Assert.Equal("Food", expense.Category);
        Assert.True(body.ClarificationNeeded);
        Assert.Equal("What amount did you spend on petrol?", body.ClarificationQuestion);
    }
}
