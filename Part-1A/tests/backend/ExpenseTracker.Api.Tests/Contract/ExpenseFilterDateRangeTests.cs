using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseFilterDateRangeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseFilterDateRangeTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Get_FilterByDateRange_ReturnsOnlyExpensesInclusiveOfBounds()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);
        var threeDaysAgo = today.AddDays(-3);

        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(100, "Food", threeDaysAgo, "out of range"));
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(200, "Food", twoDaysAgo, "lower bound"));
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(300, "Food", yesterday, "upper bound"));

        var results = await _client.GetFromJsonAsync<List<ExpenseResponse>>(
            $"/api/expenses?from={twoDaysAgo:yyyy-MM-dd}&to={yesterday:yyyy-MM-dd}");

        Assert.NotNull(results);
        Assert.Equal(2, results!.Count);
        Assert.All(results, e => Assert.True(e.Date >= twoDaysAgo && e.Date <= yesterday));
    }
}
