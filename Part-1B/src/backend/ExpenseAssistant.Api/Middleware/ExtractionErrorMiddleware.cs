using ExpenseAssistant.Api.Services;

namespace ExpenseAssistant.Api.Middleware;

/// <summary>
/// Translates a GeminiApiException into 502 Bad Gateway, distinguishing a model/service
/// failure from an application-level 400 (which the controller returns directly for an
/// empty/missing message) — so harness evidence can tell the two failure modes apart
/// (assignment Common Expectations).
/// </summary>
public class ExtractionErrorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (GeminiApiException ex)
        {
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsJsonAsync(new { error = "gemini_api_failure", message = ex.Message });
        }
    }
}
