namespace ExpenseTracker.Api.Models;

public class BudgetLimit
{
    public int Id { get; set; }

    public Category Category { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public int LimitAmount { get; set; }
}
