using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Xunit;

namespace ExpenseTracker.Api.Tests.Contract;

public class ExpenseCreateTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpenseCreateTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Post_ValidExpense_Returns201WithCreatedExpense_WithinTwoSeconds()
    {
        // Warm up the host, EF Core model building, and the create/query code paths (first use
        // of each triggers one-time JIT/model-compilation cost that is not representative of
        // steady-state request latency — the thing SC-001 actually measures).
        for (var i = 0; i < 3; i++)
        {
            await _client.GetAsync("/api/expenses");
        }

        await _client.PostAsJsonAsync(
            "/api/expenses", new ExpenseRequest(1, "Other", DateOnly.FromDateTime(DateTime.UtcNow), null));

        var request = new ExpenseRequest(500, "Food", DateOnly.FromDateTime(DateTime.UtcNow), "Lunch");

        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsJsonAsync("/api/expenses", request);
        stopwatch.Stop();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), "SC-001: response must complete in under 2 seconds");

        var created = await response.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(created);
        Assert.Equal(500, created!.Amount);
        Assert.Equal("Food", created.Category);
        Assert.Equal("Lunch", created.Description);
        Assert.True(created.Id > 0);
    }
}
