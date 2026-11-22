using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class SummaryNoBudgetSetTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SummaryNoBudgetSetTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Get_Summary_NoAlertWhenNoBudgetLimitSet()
    {
        var date = new DateOnly(2026, 10, 10);
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(5000, "Entertainment", date, null));

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>("/api/summary?year=2026&month=10");

        var entertainment = Assert.Single(summary!.ByCategory, c => c.Category == "Entertainment");
        Assert.Null(entertainment.BudgetLimit);
        Assert.False(entertainment.OverBudget);
    }
}
