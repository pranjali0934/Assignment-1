using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class BudgetSetTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BudgetSetTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Post_SetBudget_Returns200AndReplacesOnResubmit()
    {
        var first = await _client.PostAsJsonAsync(
            "/api/budgets", new BudgetLimitRequest("Food", 2026, 7, 1000));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstBody = await first.Content.ReadFromJsonAsync<BudgetLimitResponse>();
        Assert.Equal(1000, firstBody!.LimitAmount);

        var second = await _client.PostAsJsonAsync(
            "/api/budgets", new BudgetLimitRequest("Food", 2026, 7, 1500));
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondBody = await second.Content.ReadFromJsonAsync<BudgetLimitResponse>();
        Assert.Equal(1500, secondBody!.LimitAmount);

        var list = await _client.GetFromJsonAsync<List<BudgetLimitResponse>>("/api/budgets?year=2026&month=7");
        Assert.Single(list!);
    }

    [Fact]
    public async Task Post_SetBudget_AllowsFutureMonth()
    {
        var futureYear = DateTime.UtcNow.Year + 1;

        var response = await _client.PostAsJsonAsync(
            "/api/budgets", new BudgetLimitRequest("Food", futureYear, 1, 1000));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
