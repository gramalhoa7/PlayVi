using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayViAPI.Data;
using PlayViAPI.Models;

namespace PlayViAPI.Controllers;

public record ProfileRequest(string Name, string? AvatarUrl, bool IsKids);

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private const int MaxProfiles = 5;
    private readonly APPDbContext _db;

    public ProfilesController(APPDbContext db) => _db = db;

    private string? UserId => User.FindFirst("sub")?.Value;

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var profiles = await _db.Profiles
            .Where(p => p.UserId == UserId)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new { p.Id, p.Name, p.AvatarUrl, p.IsKids })
            .ToListAsync();

        return Ok(profiles);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProfileRequest req)
    {
        if (UserId is null) return Unauthorized();

        var name = req.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 20)
            return BadRequest(new { message = "The name must be between 1 and 20 characters long." });

        var count = await _db.Profiles.CountAsync(p => p.UserId == UserId);
        if (count >= MaxProfiles)
            return BadRequest(new { message = $"Limit of {MaxProfiles} profiles per account reached." });

        var profile = new Profile
        {
            UserId = UserId,
            Name = name,
            AvatarUrl = req.AvatarUrl,
            IsKids = req.IsKids
        };

        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync();

        return Ok(new { profile.Id, profile.Name, profile.AvatarUrl, profile.IsKids });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProfileRequest req)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == id && p.UserId == UserId);
        if (profile is null) return NotFound();

        var name = req.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 20)
            return BadRequest(new { message = "The name must be between 1 and 20 characters long." });

        profile.Name = name;
        profile.AvatarUrl = req.AvatarUrl;
        profile.IsKids = req.IsKids;
        await _db.SaveChangesAsync();

        return Ok(new { profile.Id, profile.Name, profile.AvatarUrl, profile.IsKids });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == id && p.UserId == UserId);
        if (profile is null) return NotFound();

        var count = await _db.Profiles.CountAsync(p => p.UserId == UserId);
        if (count <= 1)
            return BadRequest(new { message = "The account must have at least one profile." });

        _db.Profiles.Remove(profile);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}