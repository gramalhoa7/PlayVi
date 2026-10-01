using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlayViAPI.Models;
using PlayViAPI.Services;
using PlayViAPI.Data;

namespace PlayViAPI.Controllers;

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly TokenService _tokens;
    private readonly APPDbContext _db;

    public AuthController(UserManager<ApplicationUser> users, TokenService tokens, APPDbContext db)
    {
        _users = users;
        _tokens = tokens;
        _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var user = new ApplicationUser
        {
            UserName = req.Email,
            Email = req.Email,
            DisplayName = req.DisplayName
        };

        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));
        
        _db.Profiles.Add(new Profile
        {
            UserId = user.Id,
            Name = string.IsNullOrWhiteSpace(req.DisplayName) ? req.Email.Split('@')[0] : req.DisplayName.Trim()
        });
        await _db.SaveChangesAsync();

        return Ok(new { token = _tokens.CreateToken(user) });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _users.FindByEmailAsync(req.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { message = "E-mail ou senha incorretos." });

        return Ok(new { token = _tokens.CreateToken(user) });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        id = User.FindFirst("sub")?.Value,
        email = User.FindFirst("email")?.Value
    });
}