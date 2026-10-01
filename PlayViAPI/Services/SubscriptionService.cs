using Microsoft.EntityFrameworkCore;
using PlayViAPI.Data;

namespace PlayViAPI.Services;

public class SubscriptionService
{
    private readonly APPDbContext _db;
    public SubscriptionService(APPDbContext db) => _db = db;

    public Task<bool> HasActiveAsync(string userId) =>
        _db.Subscriptions.AnyAsync(s =>
            s.UserId == userId &&
            s.Status == "Active" &&
            s.EndDate > DateTime.UtcNow);
}