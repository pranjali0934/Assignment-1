using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class SummaryOverBudgetTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SummaryOverBudgetTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Get_Summary_ReportsOverBudgetWhenSpendingExceedsLimit()
    {
        var date = new DateOnly(2026, 8, 10);
        await _client.PostAsJsonAsync("/api/budgets", new BudgetLimitRequest("Food", 2026, 8, 500));
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(300, "Food", date, null));
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(400, "Food", date, null));

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>("/api/summary?year=2026&month=8");

        var food = Assert.Single(summary!.ByCategory, c => c.Category == "Food");
        Assert.Equal(500, food.BudgetLimit);
        Assert.True(food.OverBudget);
    }

    [Fact]
    public async Task Get_Summary_NotOverBudgetWhenSpendingWithinLimit()
    {
        var date = new DateOnly(2026, 9, 10);
        await _client.PostAsJsonAsync("/api/budgets", new BudgetLimitRequest("Transport", 2026, 9, 1000));
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(300, "Transport", date, null));

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>("/api/summary?year=2026&month=9");

        var transport = Assert.Single(summary!.ByCategory, c => c.Category == "Transport");
        Assert.False(transport.OverBudget);
    }
}
