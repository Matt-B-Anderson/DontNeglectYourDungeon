using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services;

public class LinkedCharacterService(ApplicationDbContext db) : ILinkedCharacterService
{
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found.");
    }
    public async Task<List<LinkedCharacter>> GetForUserAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        return await db.LinkedCharacters
            .Where(l => l.OwnerUserId == userId)
            .OrderBy(l => l.DisplayName)
            .ToListAsync();
    }

    public async Task<LinkedCharacter> CreateAsync(LinkedCharacter link, ClaimsPrincipal user)
    {
        link.OwnerUserId = GetUserId(user);

        // Basic validation for Beyond links
        if (!link.Url.StartsWith("https://www.dndbeyond.com/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("URL must start with https://www.dndbeyond.com/");

        db.LinkedCharacters.Add(link);
        await db.SaveChangesAsync();
        return link;
    }

    public async Task<bool> UpdateAsync(LinkedCharacter link, ClaimsPrincipal user)
    {
        var existing = await db.LinkedCharacters.FirstOrDefaultAsync(l => l.Id == link.Id);
        var userId = GetUserId(user);
        if (existing is null || existing.OwnerUserId != userId) return false;

        if (!link.Url.StartsWith("https://www.dndbeyond.com/", StringComparison.OrdinalIgnoreCase))
            return false;

        existing.DisplayName = link.DisplayName;
        existing.Url = link.Url;

        await db.SaveChangesAsync();
        return true;
    }

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
