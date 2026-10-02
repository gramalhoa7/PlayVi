using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayViAPI.Data;

namespace PlayViAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly APPDbContext _db;
    public GenresController(APPDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await _db.Genres.OrderBy(g => g.Name).Select(g => g.Name).ToListAsync());
}