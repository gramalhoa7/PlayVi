using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayViAPI.Data;

namespace PlayViAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase
{
    private readonly APPDbContext _db;
    public PlansController(APPDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _db.Plans.ToListAsync());
}