using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseFilterCategoryTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseFilterCategoryTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Get_FilterByCategory_ReturnsOnlyMatchingExpenses()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(500, "Food", today, null));
        await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(200, "Transport", today, null));

        var results = await _client.GetFromJsonAsync<List<ExpenseResponse>>("/api/expenses?category=Food");

        Assert.NotNull(results);
        Assert.All(results!, e => Assert.Equal("Food", e.Category));
    }
}
