using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayViAPI.Data;
using PlayViAPI.Services;

namespace PlayViAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TitlesController : ControllerBase
{
    private readonly APPDbContext _db;
    private readonly SubscriptionService _subs;

    public TitlesController(APPDbContext db, SubscriptionService subs)
    {
        _db = db;
        _subs = subs;
    }

    // GET /api/titles?q=teste&genre=Comedy&era=antigos&type=Movie
    [HttpGet]
    public async Task<IActionResult> Search(string? q, string? genre, string? era, string? type)
    {
        var query = _db.Titles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Name.Contains(q));

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(t => t.TitleGenres.Any(g => g.Genre.Name == genre));

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(t => t.Type == type);

        query = era?.ToLower() switch
        {
            "antigos" => query.Where(t => t.ReleaseYear < 2000),
            "novos"   => query.Where(t => t.ReleaseYear >= 2020),
            _         => query
        };

        var results = await query
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Type,
                t.ReleaseYear,
                t.Description,
                t.PosterUrl,
                Genres = t.TitleGenres.Select(g => g.Genre.Name)
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET /api/titles/1/watch
    [HttpGet("{id}/watch")]
    public async Task<IActionResult> Watch(int id)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId is null || !await _subs.HasActiveAsync(userId))
            return StatusCode(403, new { message = "Você precisa de uma assinatura ativa para assistir." });

        var title = await _db.Titles.FindAsync(id);
        if (title is null) return NotFound();

        return Ok(new { title.Id, title.Name, title.VideoUrl });
    }
}