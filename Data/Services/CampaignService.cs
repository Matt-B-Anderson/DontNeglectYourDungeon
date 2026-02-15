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

    private async Task<string> GenerateUniqueJoinCodeAsync()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        string code;

        do
        {
            code = new string(Enumerable.Range(0, 8)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
        while (await db.Campaigns.AnyAsync(c => c.JoinCode == code));

        return code;
    }
    public async Task<List<Campaign>> GetForUserAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);

        // First, ensure all owned campaigns have member records
        var ownedCampaigns = await db.Campaigns
            .Where(c => c.OwnerUserId == userId)
            .ToListAsync();

        foreach (var campaign in ownedCampaigns)
        {
            var memberExists = await db.CampaignMembers
                .AnyAsync(cm => cm.CampaignId == campaign.Id && cm.UserId == userId);

            if (!memberExists)
            {
                db.CampaignMembers.Add(new CampaignMember
                {
                    CampaignId = campaign.Id,
                    UserId = userId,
                    JoinedUtc = campaign.CreatedUtc
                });
            }

            // Also ensure join code exists
            if (string.IsNullOrWhiteSpace(campaign.JoinCode))
            {
                campaign.JoinCode = await GenerateUniqueJoinCodeAsync();
            }
        }

        if (ownedCampaigns.Any())
        {
            await db.SaveChangesAsync();
        }

        // Return all campaigns where user is a member
        return await db.Campaigns
            .Where(c => db.CampaignMembers.Any(cm => cm.CampaignId == c.Id && cm.UserId == userId))
            .OrderByDescending(c => c.CreatedUtc)
            .ToListAsync();
    }

    public async Task<Campaign?> GetOwnedByUserAsync(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var campaign = await db.Campaigns
            .Include(c => c.Sessions)
            .Include(c => c.Characters)
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerUserId == userId);

        // Ensure campaign has a join code (for legacy campaigns)
        if (campaign is not null && string.IsNullOrWhiteSpace(campaign.JoinCode))
        {
            campaign.JoinCode = await GenerateUniqueJoinCodeAsync();
            await db.SaveChangesAsync();
        }

        return campaign;
    }

    public async Task<Campaign?> GetByIdForMemberAsync(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);

        // Check if user is a member of this campaign
        var isMember = await db.CampaignMembers
            .AnyAsync(cm => cm.CampaignId == campaignId && cm.UserId == userId);

        if (!isMember) return null;

        var campaign = await db.Campaigns
            .Include(c => c.Sessions)
            .Include(c => c.Characters)
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        // Ensure campaign has a join code (for legacy campaigns owned by user)
        if (campaign is not null && campaign.OwnerUserId == userId && string.IsNullOrWhiteSpace(campaign.JoinCode))
        {
            campaign.JoinCode = await GenerateUniqueJoinCodeAsync();
            await db.SaveChangesAsync();
        }

        return campaign;
    }

    public async Task<Campaign> CreateAsync(Campaign campaign, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        campaign.OwnerUserId = userId;
        campaign.CreatedUtc = DateTime.UtcNow;
        campaign.JoinCode = await GenerateUniqueJoinCodeAsync();

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();

        // Automatically add owner as a member
        var member = new CampaignMember
        {
            CampaignId = campaign.Id,
            UserId = userId,
            JoinedUtc = DateTime.UtcNow
        };

        db.CampaignMembers.Add(member);
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

    public async Task<Campaign?> JoinByCodeAsync(string joinCode, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.JoinCode == joinCode.ToUpper());

        if (campaign is null) return null;

        // Check if user is already a member
        var existingMember = await db.CampaignMembers
            .FirstOrDefaultAsync(cm => cm.CampaignId == campaign.Id && cm.UserId == userId);

        if (existingMember is not null) return campaign; // Already a member

        // Add user as a member
        var member = new CampaignMember
        {
            CampaignId = campaign.Id,
            UserId = userId,
            JoinedUtc = DateTime.UtcNow
        };

        db.CampaignMembers.Add(member);
        await db.SaveChangesAsync();

        return campaign;
    }
}
