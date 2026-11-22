using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseAmountValidationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseAmountValidationTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Theory]
    [InlineData(500.50)]
    [InlineData(0)]
    [InlineData(-200)]
    public async Task Post_InvalidAmount_Returns400(decimal amount)
    {
        var request = new ExpenseRequest(amount, "Food", DateOnly.FromDateTime(DateTime.UtcNow), null);

        var response = await _client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100_000)]
    public async Task Post_BoundaryAmounts_AreAccepted(decimal amount)
    {
        var request = new ExpenseRequest(amount, "Food", DateOnly.FromDateTime(DateTime.UtcNow), null);

        var response = await _client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
