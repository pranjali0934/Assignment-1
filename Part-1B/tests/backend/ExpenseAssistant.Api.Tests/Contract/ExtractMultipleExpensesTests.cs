using System.Net.Http.Json;
using ExpenseAssistant.Api.Models;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Contract;

public class ExtractMultipleExpensesTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExtractMultipleExpensesTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_TwoExpensesInOneMessage_ReturnsBothCorrectlyAttributed()
    {
        _factory.Fake.Enqueue(new RawExtractionResponse(
            [
                new RawExtractedExpense(500, "Food", "2026-11-22", "groceries"),
                new RawExtractedExpense(200, "Transport", "2026-11-22", "petrol"),
            ],
            false,
            null));

        var response = await _client.PostAsJsonAsync(
            "/api/extract", new ExtractRequest("I spent ₹500 on groceries and ₹200 on petrol"));

        var body = await response.Content.ReadFromJsonAsync<ExtractResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Expenses.Count);
        Assert.Contains(body.Expenses, e => e.Amount == 500 && e.Category == "Food");
        Assert.Contains(body.Expenses, e => e.Amount == 200 && e.Category == "Transport");
    }
}
