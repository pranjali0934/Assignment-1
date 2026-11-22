using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class ExpenseService(ExpenseTrackerDbContext db, ExpenseValidationService validation)
{
    public async Task<Expense> CreateAsync(ExpenseRequest request, CancellationToken ct = default)
    {
        var amount = validation.ValidateAndParseAmount(nameof(request.Amount), request.Amount);
        validation.ValidateExpenseDate(nameof(request.Date), request.Date);
        var category = validation.ValidateCategory(nameof(request.Category), request.Category);

        var now = DateTimeOffset.UtcNow;
        var expense = new Expense
        {
            Amount = amount,
            Category = category,
            Date = request.Date,
            Description = request.Description,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync(ct);
        return expense;
    }

    public async Task<Expense> UpdateAsync(int id, ExpenseRequest request, CancellationToken ct = default)
    {
        var expense = await db.Expenses.FindAsync([id], ct)
            ?? throw new NotFoundException($"Expense {id} not found.");

        var amount = validation.ValidateAndParseAmount(nameof(request.Amount), request.Amount);
        validation.ValidateExpenseDate(nameof(request.Date), request.Date);
        var category = validation.ValidateCategory(nameof(request.Category), request.Category);

        expense.Amount = amount;
        expense.Category = category;
        expense.Date = request.Date;
        expense.Description = request.Description;
        expense.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return expense;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var expense = await db.Expenses.FindAsync([id], ct)
            ?? throw new NotFoundException($"Expense {id} not found.");

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Expense>> ListAsync(
        string? category, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        validation.ValidateDateRange(from, to);

        var query = db.Expenses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var parsedCategory = validation.ValidateCategory("category", category);
            query = query.Where(e => e.Category == parsedCategory);
        }

        if (from.HasValue)
        {
            query = query.Where(e => e.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.Date <= to.Value);
        }

        return await query.OrderByDescending(e => e.Date).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Expense>> ListForMonthAsync(int year, int month, CancellationToken ct = default)
    {
        return await db.Expenses
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .ToListAsync(ct);
    }
}
