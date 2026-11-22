using System.Net;
using System.Net.Http.Json;
using ExpenseAssistant.Api.Models;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Contract;

public class ExtractEmptyMessageTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExtractEmptyMessageTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_EmptyMessage_Returns400(string message)
    {
        var response = await _client.PostAsJsonAsync("/api/extract", new ExtractRequest(message));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
