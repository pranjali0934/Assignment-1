using ExpenseAssistant.Api.Models;
using ExpenseAssistant.Api.Services;
using Xunit;

namespace ExpenseAssistant.Api.Tests.Unit;

public class ValidationServiceTests
{
    private readonly ValidationService _sut = new();
    private static readonly DateOnly Today = new(2026, 11, 22);

    [Theory]
    [InlineData(1)]
    [InlineData(100_000)]
    [InlineData(450)]
    public void Validate_AcceptsAmountsWithinRange(decimal amount)
    {
        var raw = new RawExtractionResponse(
            [new RawExtractedExpense(amount, "Food", "2026-11-21", null)], false, null);

        var result = _sut.Validate(raw, Today);

        var expense = Assert.Single(result.Expenses);
        Assert.Equal((int)amount, expense.Amount);
        Assert.False(result.ClarificationNeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    [InlineData(100_001)]
    public void Validate_RejectsOutOfRangeAmounts(decimal amount)
    {
        var raw = new RawExtractionResponse(
            [new RawExtractedExpense(amount, "Food", "2026-11-21", null)], false, null);

        var result = _sut.Validate(raw, Today);

        Assert.Empty(result.Expenses);
        Assert.True(result.ClarificationNeeded);
    }

    [Fact]
    public void Validate_RejectsDecimalAmount()
    {
        var raw = new RawExtractionResponse(
            [new RawExtractedExpense(500.50m, "Food", "2026-11-21", null)], false, null);

        var result = _sut.Validate(raw, Today);

        Assert.Empty(result.Expenses);
        Assert.True(result.ClarificationNeeded);
    }

    [Fact]
    public void Validate_UnknownCategory_MapsToOther()
    {
        var raw = new RawExtractionResponse(
            [new RawExtractedExpense(500, "Luxury", "2026-11-21", null)], false, null);

        var result = _sut.Validate(raw, Today);

        var expense = Assert.Single(result.Expenses);
        Assert.Equal(Category.Other, expense.Category);
    }

    [Fact]
    public void Validate_MissingDate_DefaultsToRequestDate()
    {
        var raw = new RawExtractionResponse(
            [new RawExtractedExpense(500, "Food", null, null)], false, null);

        var result = _sut.Validate(raw, Today);

        var expense = Assert.Single(result.Expenses);
        Assert.Equal(Today, expense.Date);
    }

    [Fact]
    public void Validate_UnparseableDate_DefaultsToRequestDate()
    {
        var raw = new RawExtractionResponse(
            [new RawExtractedExpense(500, "Food", "not-a-date", null)], false, null);

        var result = _sut.Validate(raw, Today);

        var expense = Assert.Single(result.Expenses);
        Assert.Equal(Today, expense.Date);
    }
}
