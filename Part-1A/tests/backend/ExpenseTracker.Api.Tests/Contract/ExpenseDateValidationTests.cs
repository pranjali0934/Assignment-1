using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseDateValidationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseDateValidationTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Post_FutureDate_Returns400()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var request = new ExpenseRequest(500, "Food", futureDate, null);

        var response = await _client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_TodayDate_IsAccepted()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var request = new ExpenseRequest(500, "Food", today, null);

        var response = await _client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
