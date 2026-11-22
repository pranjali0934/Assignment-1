using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class BudgetService(ExpenseTrackerDbContext db, ExpenseValidationService validation)
{
    public async Task<BudgetLimit> SetAsync(BudgetLimitRequest request, CancellationToken ct = default)
    {
        var category = validation.ValidateCategory(nameof(request.Category), request.Category);
        var limitAmount = validation.ValidateAndParseAmount(nameof(request.LimitAmount), request.LimitAmount);

        var existing = await db.BudgetLimits.SingleOrDefaultAsync(
            b => b.Category == category && b.Year == request.Year && b.Month == request.Month, ct);

        if (existing is not null)
        {
            existing.LimitAmount = limitAmount;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var budgetLimit = new BudgetLimit
        {
            Category = category,
            Year = request.Year,
            Month = request.Month,
            LimitAmount = limitAmount,
        };

        db.BudgetLimits.Add(budgetLimit);
        await db.SaveChangesAsync(ct);
        return budgetLimit;
    }

    public async Task<IReadOnlyList<BudgetLimit>> ListForMonthAsync(int year, int month, CancellationToken ct = default)
    {
        return await db.BudgetLimits
            .Where(b => b.Year == year && b.Month == month)
            .ToListAsync(ct);
    }
}
