using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services;

public class CampaignService(ApplicationDbContext db) : ICampaignService
{
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found.");
    }
    public async Task<List<Campaign>> GetForUserAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        return await db.Campaigns
            .Where(c => c.OwnerUserId == userId)
            .OrderByDescending(c => c.CreatedUtc)
            .ToListAsync();
    }

    public async Task<Campaign?> GetOwnedByUserAsync(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        return await db.Campaigns
            .Include(c => c.Sessions)
            .Include(c => c.Characters)
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerUserId == userId);
    }

    public async Task<Campaign> CreateAsync(Campaign campaign, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        campaign.OwnerUserId = userId;
        campaign.CreatedUtc = DateTime.UtcNow;

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign;
    }

    public async Task<bool> UpdateAsync(Campaign campaign, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var existing = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaign.Id);
        if (existing is null || existing.OwnerUserId != userId) return false;

        existing.Name = campaign.Name;
        existing.System = campaign.System;
        existing.Description = campaign.Description;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var existing = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId);
        if (existing is null || existing.OwnerUserId != userId) return false;

        db.Campaigns.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
