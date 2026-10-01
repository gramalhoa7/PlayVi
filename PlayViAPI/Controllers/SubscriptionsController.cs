using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayViAPI.Data;
using PlayViAPI.Models;
using PlayViAPI.Services;

namespace PlayViAPI.Controllers;

public record SubscribeRequest(int PlanId);

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly APPDbContext _db;
    private readonly SubscriptionService _subs;

    public SubscriptionsController(APPDbContext db, SubscriptionService subs)
    {
        _db = db;
        _subs = subs;
    }

    private string? UserId => User.FindFirst("sub")?.Value;

    [HttpPost]
    public async Task<IActionResult> Subscribe(SubscribeRequest req)
    {
        if (UserId is null) return Unauthorized();

        if (await _subs.HasActiveAsync(UserId))
            return Conflict(new { message = "Você já tem uma assinatura ativa. Cancele antes de trocar de plano." });

        var plan = await _db.Plans.FindAsync(req.PlanId);
        if (plan is null) return NotFound(new { message = "Plano não encontrado." });

        // AQUI entraria o pagamento real (Mercado Pago/Stripe).
        // Por enquanto, consideramos que sempre foi aprovado.

        var now = DateTime.UtcNow;
        var sub = new Subscription
        {
            UserId = UserId,
            PlanId = plan.Id,
            StartDate = now,
            EndDate = plan.BillingPeriod == "Yearly" ? now.AddYears(1) : now.AddMonths(1),
            Status = "Active"
        };

        _db.Subscriptions.Add(sub);
        await _db.SaveChangesAsync();

        return Ok(new { plan = plan.Name, sub.StartDate, sub.EndDate, sub.Status });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Mine()
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == UserId)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();

        if (sub is null) return Ok(new { active = false });

        return Ok(new
        {
            active = sub.Status == "Active" && sub.EndDate > DateTime.UtcNow,
            plan = sub.Plan.Name,
            sub.StartDate,
            sub.EndDate,
            sub.Status
        });
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel()
    {
        var sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == UserId && s.Status == "Active");

        if (sub is null) return NotFound(new { message = "Nenhuma assinatura ativa." });

        sub.Status = "Canceled";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Assinatura cancelada." });
    }
}