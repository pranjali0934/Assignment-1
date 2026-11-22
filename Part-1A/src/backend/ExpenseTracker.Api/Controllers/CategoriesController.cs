using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<string>> Get() => Ok(Enum.GetNames<Category>());
}
