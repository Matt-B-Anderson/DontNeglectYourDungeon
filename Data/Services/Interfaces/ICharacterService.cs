using DontNeglectYourDungeon.Data.Models;

namespace DontNeglectYourDungeon.Data.Services.Interfaces;

public interface ICharacterService
{
    Task<List<Models.Character>> GetForCampaignAsync(int campaignId, string currentUserId);
    Task<Models.Character?> GetByIdAsync(int characterId, string currentUserId);

    Task<Models.Character> CreateAsync(Models.Character character, string currentUserId);
    Task<bool> UpdateAsync(Models.Character character, string currentUserId);
    Task<bool> DeleteAsync(int characterId, string currentUserId);
}
