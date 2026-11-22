using ExpenseAssistant.Api.Models;
using ExpenseAssistant.Api.Services;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Unit;

/// <summary>
/// Focused tests for FR-006/FR-009: an undeterminable amount must never be guessed — it must
/// drop the expense and request clarification instead.
/// </summary>
public class NoFabricationGuardTests
{
    private readonly ValidationService _sut = new();
    private static readonly DateOnly Today = new(2026, 11, 22);

    [Fact]
    public void Validate_NullAmount_DropsExpenseAndRequestsClarification()
    {
        var raw = new RawExtractionResponse(
            [new RawExtractedExpense(null, "Food", "2026-11-21", "dinner with friends")], false, null);

        var result = _sut.Validate(raw, Today);

        Assert.Empty(result.Expenses);
        Assert.True(result.ClarificationNeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.ClarificationQuestion));
    }

    [Fact]
    public void Validate_PartialSuccess_KeepsWellFormedExpenseAndFlagsIncompleteOne()
    {
        var raw = new RawExtractionResponse(
            [
                new RawExtractedExpense(500, "Food", "2026-11-21", "groceries"),
                new RawExtractedExpense(null, "Transport", "2026-11-21", "petrol"),
            ],
            false,
            null);

        var result = _sut.Validate(raw, Today);

        var expense = Assert.Single(result.Expenses);
        Assert.Equal(500, expense.Amount);
        Assert.True(result.ClarificationNeeded);
    }

    [Fact]
    public void Validate_ModelsOwnClarificationQuestion_IsPreservedOverDefault()
    {
        var raw = new RawExtractionResponse(
            [new RawExtractedExpense(null, "Food", null, null)],
            true,
            "What specific amount did you spend on food?");

        var result = _sut.Validate(raw, Today);

        Assert.Equal("What specific amount did you spend on food?", result.ClarificationQuestion);
    }

    [Fact]
    public void Validate_NoExpensesAndModelFlagsClarification_NeverFabricatesOne()
    {
        var raw = new RawExtractionResponse([], true, "I couldn't find an expense in your message.");

        var result = _sut.Validate(raw, Today);

        Assert.Empty(result.Expenses);
        Assert.True(result.ClarificationNeeded);
    }
}
