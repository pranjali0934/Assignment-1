using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services;

/// <summary>
/// Computes the monthly summary by recomputing live from current Expense/BudgetLimit data on
/// every call (no caching/stored alert state), per spec FR-012 and data-model.md.
/// </summary>
public class SummaryService(ExpenseService expenseService, BudgetService budgetService)
{
    public async Task<MonthlySummaryResponse> GetMonthlySummaryAsync(int year, int month, CancellationToken ct = default)
    {
        var expenses = await expenseService.ListForMonthAsync(year, month, ct);
        var budgetLimits = await budgetService.ListForMonthAsync(year, month, ct);
        var budgetByCategory = budgetLimits.ToDictionary(b => b.Category, b => b.LimitAmount);

        var byCategory = expenses
            .GroupBy(e => e.Category)
            .Select(g =>
            {
                var total = g.Sum(e => e.Amount);
                var hasLimit = budgetByCategory.TryGetValue(g.Key, out var limit);
                var overBudget = hasLimit && total > limit;
                return new CategorySummary(
                    g.Key.ToString(),
                    total,
                    hasLimit ? limit : null,
                    overBudget);
            })
            .OrderBy(c => c.Category)
            .ToList();

        var total = expenses.Sum(e => e.Amount);
        return new MonthlySummaryResponse(year, month, total, byCategory);
    }
}
