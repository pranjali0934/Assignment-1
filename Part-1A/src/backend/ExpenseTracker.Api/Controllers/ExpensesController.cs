using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController(ExpenseService expenseService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> Create(ExpenseRequest request, CancellationToken ct)
    {
        var expense = await expenseService.CreateAsync(request, ct);
        var response = ExpenseResponse.FromEntity(expense);
        return CreatedAtAction(nameof(Create), new { id = expense.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponse>>> List(
        [FromQuery] string? category,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var expenses = await expenseService.ListAsync(category, from, to, ct);
        return Ok(expenses.Select(ExpenseResponse.FromEntity));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> Update(int id, ExpenseRequest request, CancellationToken ct)
    {
        var expense = await expenseService.UpdateAsync(id, request, ct);
        return Ok(ExpenseResponse.FromEntity(expense));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await expenseService.DeleteAsync(id, ct);
        return NoContent();
    }
}
