using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DontNeglectYourDungeon.Data.Services;

public class CampaignService(ApplicationDbContext db) : ICampaignService
{
    public async Task<List<Campaign>> GetForUserAsync(string currentUserId)
    {
        return await db.Campaigns
            .Where(c => c.OwnerUserId == currentUserId)
            .OrderByDescending(c => c.CreatedUtc)
            .ToListAsync();
    }

    public async Task<Campaign?> GetOwnedByUserAsync(int campaignId, string currentUserId)
    {
        return await db.Campaigns
            .Include(c => c.Sessions)
            .Include(c => c.Characters)
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerUserId == currentUserId);
    }

    public async Task<Campaign> CreateAsync(Campaign campaign, string currentUserId)
    {
        campaign.OwnerUserId = currentUserId;
        campaign.CreatedUtc = DateTime.UtcNow;

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign;
    }

    public async Task<bool> UpdateAsync(Campaign campaign, string currentUserId)
    {
        var existing = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaign.Id);
        if (existing is null || existing.OwnerUserId != currentUserId) return false;

        existing.Name = campaign.Name;
        existing.System = campaign.System;
        existing.Description = campaign.Description;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int campaignId, string currentUserId)
    {
        var existing = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId);
        if (existing is null || existing.OwnerUserId != currentUserId) return false;

        db.Campaigns.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
