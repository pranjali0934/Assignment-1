using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseTracker.Api.Tests.Unit;

public class SummaryServiceTests
{
    private static SummaryService CreateSut(out ExpenseTrackerDbContext db)
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        db = new ExpenseTrackerDbContext(options);
        var validation = new ExpenseValidationService();
        var expenseService = new ExpenseService(db, validation);
        var budgetService = new BudgetService(db, validation);
        return new SummaryService(expenseService, budgetService);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_AggregatesTotalsPerCategory()
    {
        var sut = CreateSut(out var db);
        db.Expenses.AddRange(
            new Expense { Amount = 300, Category = Category.Food, Date = new DateOnly(2026, 6, 1) },
            new Expense { Amount = 400, Category = Category.Food, Date = new DateOnly(2026, 6, 15) },
            new Expense { Amount = 100, Category = Category.Transport, Date = new DateOnly(2026, 6, 20) });
        await db.SaveChangesAsync();

        var summary = await sut.GetMonthlySummaryAsync(2026, 6);

        Assert.Equal(800, summary.Total);
        Assert.Equal(700, summary.ByCategory.Single(c => c.Category == "Food").Total);
        Assert.Equal(100, summary.ByCategory.Single(c => c.Category == "Transport").Total);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_NoBudgetLimit_ReportsNullLimitAndNotOverBudget()
    {
        var sut = CreateSut(out var db);
        db.Expenses.Add(new Expense { Amount = 5000, Category = Category.Entertainment, Date = new DateOnly(2026, 10, 1) });
        await db.SaveChangesAsync();

        var summary = await sut.GetMonthlySummaryAsync(2026, 10);

        var entertainment = summary.ByCategory.Single(c => c.Category == "Entertainment");
        Assert.Null(entertainment.BudgetLimit);
        Assert.False(entertainment.OverBudget);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_SpendingExceedsLimit_ReportsOverBudget()
    {
        var sut = CreateSut(out var db);
        db.Expenses.Add(new Expense { Amount = 600, Category = Category.Food, Date = new DateOnly(2026, 8, 1) });
        db.BudgetLimits.Add(new BudgetLimit { Category = Category.Food, Year = 2026, Month = 8, LimitAmount = 500 });
        await db.SaveChangesAsync();

        var summary = await sut.GetMonthlySummaryAsync(2026, 8);

        var food = summary.ByCategory.Single(c => c.Category == "Food");
        Assert.Equal(500, food.BudgetLimit);
        Assert.True(food.OverBudget);
    }
}
