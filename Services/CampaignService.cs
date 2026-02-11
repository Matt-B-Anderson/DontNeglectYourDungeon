using DontNeglectYourDungeon.Data;
using DontNeglectYourDungeon.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Services;

public sealed class CampaignService : ICampaignService
{
    private readonly ApplicationDbContext _db;

    public CampaignService(ApplicationDbContext db)
    {
        _db = db;
    }

    private static string? GetUserId(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<List<Campaign>> GetMineAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return [];

        return await _db.Campaigns
            .AsNoTracking()
            .Where(c => c.OwnerId == userId)
            .OrderByDescending(c => c.UpdatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Campaign?> GetMineByIdAsync(ClaimsPrincipal user, int campaignId, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return await _db.Campaigns
            .Include(c => c.Sessions)
            .Include(c => c.CharacterLinks)
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerId == userId, ct);
    }

    public async Task<int> CreateAsync(ClaimsPrincipal user, string name, string? description, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("User is not authenticated.");

        var now = DateTime.UtcNow;

        var campaign = new Campaign
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            OwnerId = userId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync(ct);
        return campaign.Id;
    }

    public async Task<bool> UpdateAsync(ClaimsPrincipal user, int campaignId, string name, string? description, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var campaign = await _db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerId == userId, ct);
        if (campaign is null)
            return false;

        campaign.Name = name.Trim();
        campaign.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        campaign.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(ClaimsPrincipal user, int campaignId, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var campaign = await _db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerId == userId, ct);
        if (campaign is null)
            return false;

        _db.Campaigns.Remove(campaign);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
