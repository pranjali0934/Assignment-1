namespace ExpenseTracker.Api.Models;

public record ExpenseRequest(decimal Amount, string Category, DateOnly Date, string? Description);

public record ExpenseResponse(
    int Id,
    int Amount,
    string Category,
    DateOnly Date,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static ExpenseResponse FromEntity(Expense e) => new(
        e.Id, e.Amount, e.Category.ToString(), e.Date, e.Description, e.CreatedAt, e.UpdatedAt);
}

public record BudgetLimitRequest(string Category, int Year, int Month, decimal LimitAmount);

public record BudgetLimitResponse(int Id, string Category, int Year, int Month, int LimitAmount)
{
    public static BudgetLimitResponse FromEntity(BudgetLimit b) => new(
        b.Id, b.Category.ToString(), b.Year, b.Month, b.LimitAmount);
}

public record CategorySummary(string Category, int Total, int? BudgetLimit, bool OverBudget);

public record MonthlySummaryResponse(int Year, int Month, int Total, IReadOnlyList<CategorySummary> ByCategory);
