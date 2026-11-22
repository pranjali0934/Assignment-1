using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController(BudgetService budgetService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BudgetLimitResponse>> Set(BudgetLimitRequest request, CancellationToken ct)
    {
        var budgetLimit = await budgetService.SetAsync(request, ct);
        return Ok(BudgetLimitResponse.FromEntity(budgetLimit));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetLimitResponse>>> List(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var limits = await budgetService.ListForMonthAsync(year, month, ct);
        return Ok(limits.Select(BudgetLimitResponse.FromEntity));
    }
}
