using DontNeglectYourDungeon.Data.Models;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services.Interfaces;

public interface ILinkedCharacterService
{
    Task<List<LinkedCharacter>> GetForUserAsync(ClaimsPrincipal user);
    Task<List<LinkedCharacter>> GetForCampaignAsync(int campaignId, ClaimsPrincipal user);

    Task<LinkedCharacter> CreateAsync(LinkedCharacter link, ClaimsPrincipal user);
    Task<bool> UpdateAsync(LinkedCharacter link, ClaimsPrincipal user);
    Task<bool> DeleteAsync(int linkedCharacterId, ClaimsPrincipal user);
}
