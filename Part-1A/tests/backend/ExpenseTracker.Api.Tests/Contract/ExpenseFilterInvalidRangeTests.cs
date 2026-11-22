using System.Net;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseFilterInvalidRangeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseFilterInvalidRangeTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Get_FromAfterTo_Returns400()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        var response = await _client.GetAsync($"/api/expenses?from={today:yyyy-MM-dd}&to={yesterday:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
