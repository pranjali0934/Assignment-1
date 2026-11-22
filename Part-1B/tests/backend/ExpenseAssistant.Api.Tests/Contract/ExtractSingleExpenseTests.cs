using System.Net;
using System.Net.Http.Json;
using ExpenseAssistant.Api.Models;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Contract;

public class ExtractSingleExpenseTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExtractSingleExpenseTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_ValidSingleExpense_Returns200WithCorrectFields()
    {
        _factory.Fake.Enqueue(new RawExtractionResponse(
            [new RawExtractedExpense(450, "Food", "2026-11-21", "dinner")],
            false,
            null));

        var response = await _client.PostAsJsonAsync(
            "/api/extract", new ExtractRequest("I spent 450 rupees on dinner yesterday"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ExtractResponse>();
        Assert.NotNull(body);
        var expense = Assert.Single(body!.Expenses);
        Assert.Equal(450, expense.Amount);
        Assert.Equal("Food", expense.Category);
        Assert.Equal(new DateOnly(2026, 11, 21), expense.Date);
        Assert.False(body.ClarificationNeeded);
    }
}
