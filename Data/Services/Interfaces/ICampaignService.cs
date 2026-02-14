using DontNeglectYourDungeon.Data.Models;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services.Interfaces;

/// <summary>
/// UI/Auth note: All methods require userId.
/// Authorization is enforced here (owner-only).
/// </summary>
public interface ICampaignService
{
    Task<List<Campaign>> GetForUserAsync(ClaimsPrincipal user);
    Task<Campaign?> GetOwnedByUserAsync(int campaignId, ClaimsPrincipal user);

    Task<Campaign> CreateAsync(Campaign campaign, ClaimsPrincipal user);
    Task<bool> UpdateAsync(Campaign campaign, ClaimsPrincipal user);
    Task<bool> DeleteAsync(int campaignId, ClaimsPrincipal user);
}
