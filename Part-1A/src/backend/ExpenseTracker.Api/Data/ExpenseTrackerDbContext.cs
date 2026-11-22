using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Data;

public class ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options)
    : DbContext(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<BudgetLimit> BudgetLimits => Set<BudgetLimit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<BudgetLimit>(entity =>
        {
            entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(e => new { e.Category, e.Year, e.Month }).IsUnique();
        });
    }
}
