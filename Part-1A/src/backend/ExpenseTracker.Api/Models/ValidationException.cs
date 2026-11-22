namespace ExpenseTracker.Api.Models;

/// <summary>
/// Thrown by services when input fails a business rule. Caught by the error-handling
/// middleware and translated into the contract's { "errors": { field: [messages] } } shape.
/// </summary>
public class AppValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException(string field, string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]> { [field] = [message] };
    }

    public AppValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }
}

public class NotFoundException(string message) : Exception(message);
