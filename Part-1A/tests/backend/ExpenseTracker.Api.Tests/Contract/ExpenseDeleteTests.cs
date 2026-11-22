using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseDeleteTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseDeleteTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Delete_ExistingExpense_Returns204AndRemovesIt()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var created = await _client.PostAsJsonAsync("/api/expenses", new ExpenseRequest(500, "Food", today, null));
        var createdExpense = await created.Content.ReadFromJsonAsync<ExpenseResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/expenses/{createdExpense!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetFromJsonAsync<List<ExpenseResponse>>("/api/expenses");
        Assert.DoesNotContain(listResponse!, e => e.Id == createdExpense.Id);
    }

    [Fact]
    public async Task Delete_NonExistentExpense_Returns404()
    {
        var response = await _client.DeleteAsync("/api/expenses/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
