namespace ExpenseTracker.Api.Models;

public class Expense
{
    public int Id { get; set; }

    public int Amount { get; set; }

    public Category Category { get; set; }

    public DateOnly Date { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
