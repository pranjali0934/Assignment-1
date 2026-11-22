using System.Text.Json;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Middleware;

/// <summary>
/// Translates AppValidationException/NotFoundException into the API contract's error shape:
/// { "errors": { "&lt;field&gt;": ["&lt;message&gt;"] } } for 400, or a bare 404 for not-found.
/// </summary>
public class ValidationExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { errors = ex.Errors }));
        }
        catch (NotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
