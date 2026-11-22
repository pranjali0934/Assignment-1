namespace ExpenseAssistant.Api.Models;

public class ExtractedExpense
{
    public int Amount { get; set; }

    public Category Category { get; set; }

    public DateOnly Date { get; set; }

    public string? Description { get; set; }
}
