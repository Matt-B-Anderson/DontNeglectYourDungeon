using DontNeglectYourDungeon.Data.Models;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services.Interfaces;

public interface ICharacterService
{
    Task<List<Models.Character>> GetForCampaignAsync(int campaignId, ClaimsPrincipal user);
    Task<Models.Character?> GetByIdAsync(int characterId, ClaimsPrincipal user);

    Task<Models.Character> CreateAsync(Models.Character character, ClaimsPrincipal user);
    Task<bool> UpdateAsync(Models.Character character, ClaimsPrincipal user);
    Task<bool> DeleteAsync(int characterId, ClaimsPrincipal user);
}
