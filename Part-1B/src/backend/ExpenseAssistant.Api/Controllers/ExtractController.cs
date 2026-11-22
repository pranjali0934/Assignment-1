using ExpenseAssistant.Api.Models;
using ExpenseAssistant.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtractController(ExtractionService extractionService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ExtractResponse>> Post(ExtractRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { errors = new { message = new[] { "message is required." } } });
        }

        return Ok(await extractionService.ExtractAsync(request.Message, ct));
    }
}
