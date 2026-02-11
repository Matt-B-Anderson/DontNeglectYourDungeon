using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DontNeglectYourDungeon.Data.Services;

public class LinkedCharacterService(ApplicationDbContext db) : ILinkedCharacterService
{
    public async Task<List<LinkedCharacter>> GetForUserAsync(string currentUserId)
    {
        return await db.LinkedCharacters
            .Where(l => l.OwnerUserId == currentUserId)
            .OrderBy(l => l.DisplayName)
            .ToListAsync();
    }

    public async Task<LinkedCharacter> CreateAsync(LinkedCharacter link, string currentUserId)
    {
        link.OwnerUserId = currentUserId;

        // Basic validation for Beyond links
        if (!link.Url.StartsWith("https://www.dndbeyond.com/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("URL must start with https://www.dndbeyond.com/");

        db.LinkedCharacters.Add(link);
        await db.SaveChangesAsync();
        return link;
    }

    public async Task<bool> UpdateAsync(LinkedCharacter link, string currentUserId)
    {
        var existing = await db.LinkedCharacters.FirstOrDefaultAsync(l => l.Id == link.Id);
        if (existing is null || existing.OwnerUserId != currentUserId) return false;

        if (!link.Url.StartsWith("https://www.dndbeyond.com/", StringComparison.OrdinalIgnoreCase))
            return false;

        existing.DisplayName = link.DisplayName;
        existing.Url = link.Url;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int linkedCharacterId, string currentUserId)
    {
        var existing = await db.LinkedCharacters.FirstOrDefaultAsync(l => l.Id == linkedCharacterId);
        if (existing is null || existing.OwnerUserId != currentUserId) return false;

        db.LinkedCharacters.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
