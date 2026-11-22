using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services;

/// <summary>
/// Enforces the constitution/data-model amount, date, and category rules shared by expenses
/// and budget limits. Validation lives here (not in controllers) so it is the single source
/// of truth, per Constitution Principle III (Validation at the Boundary).
/// </summary>
public class ExpenseValidationService
{
    public const int MinAmount = 1;
    public const int MaxAmount = 100_000;

    /// <summary>
    /// Validates an amount received as a decimal (so a JSON number like 500.50 is detectable as
    /// non-integer, rather than being silently rounded/truncated by model binding) and returns
    /// the validated integer value. Throws AppValidationException on any rule violation.
    /// </summary>
    public int ValidateAndParseAmount(string field, decimal rawAmount)
    {
        if (rawAmount != Math.Truncate(rawAmount))
        {
            throw new AppValidationException(
                field,
                $"{field} must be a whole number (decimal values are not allowed).");
        }

        var amount = (int)rawAmount;
        ValidateAmount(field, amount);
        return amount;
    }

    public void ValidateAmount(string field, int amount)
    {
        if (amount < MinAmount || amount > MaxAmount)
        {
            throw new AppValidationException(
                field,
                $"{field} must be between {MinAmount} and {MaxAmount} (inclusive).");
        }
    }

    public void ValidateExpenseDate(string field, DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (date > today)
        {
            throw new AppValidationException(field, $"{field} cannot be in the future.");
        }
    }

    public Category ValidateCategory(string field, string? rawCategory)
    {
        if (string.IsNullOrWhiteSpace(rawCategory) || !Enum.TryParse<Category>(rawCategory, ignoreCase: true, out var category))
        {
            throw new AppValidationException(
                field,
                $"{field} must be one of: {string.Join(", ", Enum.GetNames<Category>())}.");
        }

        return category;
    }

    public void ValidateDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            throw new AppValidationException("from", "'from' must not be after 'to'.");
        }
    }
}
