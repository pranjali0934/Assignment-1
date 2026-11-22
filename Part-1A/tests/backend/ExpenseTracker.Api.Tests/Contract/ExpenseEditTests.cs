using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseEditTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseEditTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Put_ExistingExpense_UpdatesAndReturns200()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var created = await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(500, "Food", today, "Lunch"));
        var createdExpense = await created.Content.ReadFromJsonAsync<ExpenseResponse>();

        var response = await _client.PutAsJsonAsync(
            $"/api/expenses/{createdExpense!.Id}", new ExpenseRequest(750, "Food", today, "Dinner"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.Equal(750, updated!.Amount);
        Assert.Equal("Dinner", updated.Description);
    }

    [Fact]
    public async Task Put_NonExistentExpense_Returns404()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/expenses/999999", new ExpenseRequest(500, "Food", DateOnly.FromDateTime(DateTime.UtcNow), null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
