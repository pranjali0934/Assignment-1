using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummaryController(SummaryService summaryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MonthlySummaryResponse>> Get(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        return Ok(await summaryService.GetMonthlySummaryAsync(year, month, ct));
    }
}
