using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class SummaryTotalsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SummaryTotalsTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Get_Summary_ReturnsCorrectTotalAndPerCategoryBreakdown()
    {
        var date = new DateOnly(2026, 6, 15);
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(300, "Food", date, null));
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(400, "Food", date, null));
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(100, "Transport", date, null));

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>("/api/summary?year=2026&month=6");

        Assert.NotNull(summary);
        Assert.Equal(800, summary!.Total);
        var food = Assert.Single(summary.ByCategory, c => c.Category == "Food");
        Assert.Equal(700, food.Total);
    }
}
