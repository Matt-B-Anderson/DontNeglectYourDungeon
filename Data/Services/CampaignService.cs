using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services;

/// <summary>
/// Service for managing campaigns (D&D game sessions).
///
/// Key Concepts:
/// - Campaign Owner: The DM who created the campaign. Can edit/delete and see the join code.
/// - Campaign Member: Any user (including owner) who has joined the campaign. Can view campaign details.
/// - Join Code: 8-character code that players use to join a campaign.
/// </summary>
public class CampaignService(ApplicationDbContext db) : ICampaignService
{
    /// <summary>
    /// Helper method to extract the current user's ID from their authentication claims.
    /// Throws an exception if the user is not authenticated.
    /// </summary>
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found.");
    }

    /// <summary>
    /// Generates a random 8-character join code (letters and numbers).
    /// Ensures the code is unique by checking against existing campaigns.
    /// </summary>
    private async Task<string> GenerateUniqueJoinCodeAsync()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        string code;

        // Keep generating codes until we find one that doesn't exist
        do
        {
            code = new string(Enumerable.Range(0, 8)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
        while (await db.Campaigns.AnyAsync(c => c.JoinCode == code));

        return code;
    }
    /// <summary>
    /// Gets all campaigns where the user is a member (either as owner or player).
    /// Also fixes legacy data by ensuring owners have member records and join codes exist.
    /// </summary>
    public async Task<List<Campaign>> GetForUserAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);

        // First, ensure all owned campaigns have member records
        // (This fixes legacy data where owners weren't automatically added as members)
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

            // Also ensure join code exists (for campaigns created before join code feature)
            if (string.IsNullOrWhiteSpace(campaign.JoinCode))
            {
                campaign.JoinCode = await GenerateUniqueJoinCodeAsync();
            }
        }

        if (ownedCampaigns.Any())
        {
            await db.SaveChangesAsync();
        }

        // Return all campaigns where user is a member (owned or joined)
        return await db.Campaigns
            .Where(c => db.CampaignMembers.Any(cm => cm.CampaignId == c.Id && cm.UserId == userId))
            .OrderByDescending(c => c.CreatedUtc)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a campaign ONLY if the user is the owner (DM).
    /// Used for edit/delete operations which require ownership.
    /// </summary>
    public async Task<Campaign?> GetOwnedByUserAsync(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var campaign = await db.Campaigns
            .Include(c => c.Sessions)       // Load related sessions
            .Include(c => c.Characters)      // Load related characters
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerUserId == userId);

        // Ensure campaign has a join code (for legacy campaigns)
        if (campaign is not null && string.IsNullOrWhiteSpace(campaign.JoinCode))
        {
            campaign.JoinCode = await GenerateUniqueJoinCodeAsync();
            await db.SaveChangesAsync();
        }

        return campaign;
    }

    /// <summary>
    /// Gets a campaign if the user is a member (owner OR player).
    /// Used for viewing campaign details - members can view but only owners can edit.
    /// </summary>
    public async Task<Campaign?> GetByIdForMemberAsync(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);

        // Check if user is a member of this campaign (either owner or joined player)
        var isMember = await db.CampaignMembers
            .AnyAsync(cm => cm.CampaignId == campaignId && cm.UserId == userId);

        if (!isMember) return null; // Not a member, no access

        var campaign = await db.Campaigns
            .Include(c => c.Sessions)       // Load related sessions
            .Include(c => c.Characters)      // Load related characters
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        // Ensure campaign has a join code (for legacy campaigns owned by user)
        if (campaign is not null && campaign.OwnerUserId == userId && string.IsNullOrWhiteSpace(campaign.JoinCode))
        {
            campaign.JoinCode = await GenerateUniqueJoinCodeAsync();
            await db.SaveChangesAsync();
        }

        return campaign;
    }

    /// <summary>
    /// Creates a new campaign with the current user as the owner (DM).
    /// Automatically generates a join code and adds the owner as the first member.
    /// </summary>
    public async Task<Campaign> CreateAsync(Campaign campaign, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        campaign.OwnerUserId = userId;
        campaign.CreatedUtc = DateTime.UtcNow;
        campaign.JoinCode = await GenerateUniqueJoinCodeAsync();

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();

        // Automatically add owner as a member (so they appear in the campaign)
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

    /// <summary>
    /// Updates a campaign. Only the owner (DM) can update.
    /// Returns false if campaign doesn't exist or user is not the owner.
    /// </summary>
    public async Task<bool> UpdateAsync(Campaign campaign, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var existing = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaign.Id);
        if (existing is null || existing.OwnerUserId != userId) return false;

        // Only update fields that can be changed (not ID, owner, etc.)
        existing.Name = campaign.Name;
        existing.System = campaign.System;
        existing.Description = campaign.Description;

        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a campaign. Only the owner (DM) can delete.
    /// Returns false if campaign doesn't exist or user is not the owner.
    /// </summary>
    public async Task<bool> DeleteAsync(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var existing = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId);
        if (existing is null || existing.OwnerUserId != userId) return false;

        db.Campaigns.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Allows a player to join a campaign using an 8-character join code.
    /// If the user is already a member, just returns the campaign.
    /// Returns null if the join code is invalid.
    /// </summary>
    public async Task<Campaign?> JoinByCodeAsync(string joinCode, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.JoinCode == joinCode.ToUpper());

        if (campaign is null) return null; // Invalid code

        // Check if user is already a member
        var existingMember = await db.CampaignMembers
            .FirstOrDefaultAsync(cm => cm.CampaignId == campaign.Id && cm.UserId == userId);

        if (existingMember is not null) return campaign; // Already a member, no need to add again

        // Add user as a new member
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
