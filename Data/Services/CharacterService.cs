using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services;

public class CharacterService(ApplicationDbContext db) : ICharacterService
{
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found.");
    }
    private async Task<bool> CampaignOwnedByUser(int campaignId, string userId)
        => await db.Campaigns.AnyAsync(c => c.Id == campaignId && c.OwnerUserId == userId);

    public async Task<List<Data.Models.Character>> GetForCampaignAsync(int campaignId, ClaimsPrincipal user)
    {
        // DM can view all characters in their campaign
        var userId = GetUserId(user);
        if (!await CampaignOwnedByUser(campaignId, userId)) return new();

        return await db.Characters
            .Where(ch => ch.CampaignId == campaignId)
            .OrderBy(ch => ch.Name)
            .ToListAsync();
    }

    public async Task<Data.Models.Character?> GetByIdAsync(int characterId, ClaimsPrincipal user)
    {
        var ch = await db.Characters.FirstOrDefaultAsync(c => c.Id == characterId);
        if (ch is null) return null;

        var userId = GetUserId(user);
        // Owner OR DM can view
        if (ch.OwnerUserId == userId) return ch;
        return await CampaignOwnedByUser(ch.CampaignId, userId) ? ch : null;
    }

    public async Task<Data.Models.Character> CreateAsync(Data.Models.Character character, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        // Players create their own characters
        character.OwnerUserId = userId;

        db.Characters.Add(character);
        await db.SaveChangesAsync();
        return character;
    }

    public async Task<bool> UpdateAsync(Data.Models.Character character, ClaimsPrincipal user)
    {
        var existing = await db.Characters.FirstOrDefaultAsync(c => c.Id == character.Id);
        if (existing is null) return false;

        var userId = GetUserId(user);
        // Only character owner can edit
        if (existing.OwnerUserId != userId) return false;

        existing.Name = character.Name;
        existing.Class = character.Class;
        existing.Level = character.Level;
        existing.Status = character.Status;
        existing.Notes = character.Notes;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int characterId, ClaimsPrincipal user)
    {
        var existing = await db.Characters.FirstOrDefaultAsync(c => c.Id == characterId);
        if (existing is null) return false;

        var userId = GetUserId(user);
        if (existing.OwnerUserId != userId) return false;

        db.Characters.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
