using DontNeglectYourDungeon.Data.Models;

namespace DontNeglectYourDungeon.Data.Services.Interfaces;

public interface ILinkedCharacterService
{
    Task<List<LinkedCharacter>> GetForUserAsync(string currentUserId);

    Task<LinkedCharacter> CreateAsync(LinkedCharacter link, string currentUserId);
    Task<bool> UpdateAsync(LinkedCharacter link, string currentUserId);
    Task<bool> DeleteAsync(int linkedCharacterId, string currentUserId);
}
