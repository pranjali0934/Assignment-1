using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseCategoryValidationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseCategoryValidationTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Post_InvalidCategory_Returns400()
    {
        var request = new ExpenseRequest(500, "NotACategory", DateOnly.FromDateTime(DateTime.UtcNow), null);

        var response = await _client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
