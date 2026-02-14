using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DontNeglectYourDungeon.Data.Services;

public class CharacterService(ApplicationDbContext db) : ICharacterService
{
    private async Task<bool> CampaignOwnedByUser(int campaignId, string userId)
        => await db.Campaigns.AnyAsync(c => c.Id == campaignId && c.OwnerUserId == userId);

    public async Task<List<Data.Models.Character>> GetForCampaignAsync(int campaignId, string currentUserId)
    {
        // DM can view all characters in their campaign
        if (!await CampaignOwnedByUser(campaignId, currentUserId)) return new();

        return await db.Characters
            .Where(ch => ch.CampaignId == campaignId)
            .OrderBy(ch => ch.Name)
            .ToListAsync();
    }

    public async Task<Data.Models.Character?> GetByIdAsync(int characterId, string currentUserId)
    {
        var ch = await db.Characters.FirstOrDefaultAsync(c => c.Id == characterId);
        if (ch is null) return null;

        // Owner OR DM can view
        if (ch.OwnerUserId == currentUserId) return ch;
        return await CampaignOwnedByUser(ch.CampaignId, currentUserId) ? ch : null;
    }

    public async Task<Data.Models.Character> CreateAsync(Data.Models.Character character, string currentUserId)
    {
        // Players create their own characters
        character.OwnerUserId = currentUserId;

        db.Characters.Add(character);
        await db.SaveChangesAsync();
        return character;
    }

    public async Task<bool> UpdateAsync(Data.Models.Character character, string currentUserId)
    {
        var existing = await db.Characters.FirstOrDefaultAsync(c => c.Id == character.Id);
        if (existing is null) return false;

        // Only character owner can edit
        if (existing.OwnerUserId != currentUserId) return false;

        existing.Name = character.Name;
        existing.Class = character.Class;
        existing.Level = character.Level;
        existing.Status = character.Status;
        existing.Notes = character.Notes;
        existing.DndBeyondUrl = character.DndBeyondUrl;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int characterId, string currentUserId)
    {
        var existing = await db.Characters.FirstOrDefaultAsync(c => c.Id == characterId);
        if (existing is null) return false;

        if (existing.OwnerUserId != currentUserId) return false;

        db.Characters.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
