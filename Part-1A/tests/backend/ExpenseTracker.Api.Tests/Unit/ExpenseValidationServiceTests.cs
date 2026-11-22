using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Xunit;

namespace ExpenseTracker.Api.Tests.Unit;

public class ExpenseValidationServiceTests
{
    private readonly ExpenseValidationService _sut = new();

    [Theory]
    [InlineData(1)]
    [InlineData(100_000)]
    [InlineData(500)]
    public void ValidateAndParseAmount_AcceptsValidIntegersInRange(decimal amount)
    {
        var result = _sut.ValidateAndParseAmount("amount", amount);
        Assert.Equal((int)amount, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100_001)]
    public void ValidateAndParseAmount_RejectsOutOfRange(decimal amount)
    {
        Assert.Throws<AppValidationException>(() => _sut.ValidateAndParseAmount("amount", amount));
    }

    [Fact]
    public void ValidateAndParseAmount_RejectsDecimalValue()
    {
        Assert.Throws<AppValidationException>(() => _sut.ValidateAndParseAmount("amount", 500.50m));
    }

    [Fact]
    public void ValidateExpenseDate_AcceptsToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _sut.ValidateExpenseDate("date", today); // should not throw
    }

    [Fact]
    public void ValidateExpenseDate_RejectsFutureDate()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        Assert.Throws<AppValidationException>(() => _sut.ValidateExpenseDate("date", tomorrow));
    }

    [Fact]
    public void ValidateCategory_RejectsUnknownCategory()
    {
        Assert.Throws<AppValidationException>(() => _sut.ValidateCategory("category", "NotACategory"));
    }

    [Fact]
    public void ValidateCategory_AcceptsKnownCategory()
    {
        var category = _sut.ValidateCategory("category", "Food");
        Assert.Equal(Category.Food, category);
    }

    [Fact]
    public void ValidateDateRange_RejectsFromAfterTo()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Assert.Throws<AppValidationException>(() => _sut.ValidateDateRange(today, today.AddDays(-1)));
    }
}
