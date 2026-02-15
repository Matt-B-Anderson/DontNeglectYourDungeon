using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services;

/// <summary>
/// Service for managing character links (URLs to D&D Beyond character sheets).
///
/// Authorization Model:
/// - View: DMs see ALL character links in their campaign, Players see only THEIR OWN links
/// - Create: Any campaign member can create links for their character
/// - Edit/Delete: Only the link owner can edit/delete their own links
///
/// This allows DMs to see all player characters while keeping player sheets private from each other.
/// </summary>
public class LinkedCharacterService(ApplicationDbContext db) : ILinkedCharacterService
{
    /// <summary>
    /// Helper method to extract the current user's ID from their authentication claims.
    /// </summary>
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found.");
    }

    /// <summary>
    /// Gets all character links owned by the current user (across all campaigns).
    /// Used for user profile or personal character management.
    /// </summary>
    public async Task<List<LinkedCharacter>> GetForUserAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        return await db.LinkedCharacters
            .Where(l => l.OwnerUserId == userId)
            .OrderBy(l => l.DisplayName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets character links for a specific campaign.
    ///
    /// IMPORTANT AUTHORIZATION LOGIC:
    /// - If user is the campaign owner (DM): Returns ALL character links in the campaign
    /// - If user is a player: Returns only THEIR OWN character links
    /// - If user is not a campaign member: Returns empty list
    ///
    /// This allows DMs to see all player characters while keeping sheets private between players.
    /// </summary>
    public async Task<List<LinkedCharacter>> GetForCampaignAsync(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);

        // First, verify user is a campaign member (required for any access)
        var isMember = await db.CampaignMembers
            .AnyAsync(cm => cm.CampaignId == campaignId && cm.UserId == userId);

        if (!isMember) return new(); // Not a member, no access at all

        // Get the campaign to check ownership
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId);
        if (campaign is null) return new();

        // Check if this user is the DM (campaign owner)
        bool isDM = campaign.OwnerUserId == userId;

        // DMs see ALL character links, players see only their own
        if (isDM)
        {
            // DM View: All characters in the campaign
            return await db.LinkedCharacters
                .Where(l => l.CampaignId == campaignId)
                .OrderBy(l => l.DisplayName)
                .ToListAsync();
        }
        else
        {
            // Player View: Only their own characters
            return await db.LinkedCharacters
                .Where(l => l.CampaignId == campaignId && l.OwnerUserId == userId)
                .OrderBy(l => l.DisplayName)
                .ToListAsync();
        }
    }

    /// <summary>
    /// Creates a new character link.
    /// Validates that the URL is a D&D Beyond link.
    /// </summary>
    public async Task<LinkedCharacter> CreateAsync(LinkedCharacter link, ClaimsPrincipal user)
    {
        link.OwnerUserId = GetUserId(user);

        // Ensure URL is from D&D Beyond (only supported platform currently)
        if (!link.Url.StartsWith("https://www.dndbeyond.com/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("URL must start with https://www.dndbeyond.com/");

        db.LinkedCharacters.Add(link);
        await db.SaveChangesAsync();
        return link;
    }

    /// <summary>
    /// Updates a character link. Only the owner can edit.
    /// Returns false if link doesn't exist or user is not the owner.
    /// </summary>
    public async Task<bool> UpdateAsync(LinkedCharacter link, ClaimsPrincipal user)
    {
        var existing = await db.LinkedCharacters.FirstOrDefaultAsync(l => l.Id == link.Id);
        var userId = GetUserId(user);
        if (existing is null || existing.OwnerUserId != userId) return false;

        // Validate URL is still from D&D Beyond
        if (!link.Url.StartsWith("https://www.dndbeyond.com/", StringComparison.OrdinalIgnoreCase))
            return false;

        // Update editable fields
        existing.DisplayName = link.DisplayName;
        existing.Url = link.Url;

        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a character link. Only the owner can delete.
    /// Returns false if link doesn't exist or user is not the owner.
    /// </summary>
    public async Task<bool> DeleteAsync(int linkedCharacterId, ClaimsPrincipal user)
    {
        var existing = await db.LinkedCharacters.FirstOrDefaultAsync(l => l.Id == linkedCharacterId);
        var userId = GetUserId(user);
        if (existing is null || existing.OwnerUserId != userId) return false;

        db.LinkedCharacters.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
