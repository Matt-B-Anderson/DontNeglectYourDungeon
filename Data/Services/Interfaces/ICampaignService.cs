using DontNeglectYourDungeon.Data.Models;

namespace DontNeglectYourDungeon.Data.Services.Interfaces;

/// <summary>
/// UI/Auth note: All methods require currentUserId.
/// Authorization is enforced here (owner-only).
/// </summary>
public interface ICampaignService
{
    Task<List<Campaign>> GetForUserAsync(string currentUserId);
    Task<Campaign?> GetOwnedByUserAsync(int campaignId, string currentUserId);

    Task<Campaign> CreateAsync(Campaign campaign, string currentUserId);
    Task<bool> UpdateAsync(Campaign campaign, string currentUserId);
    Task<bool> DeleteAsync(int campaignId, string currentUserId);
}
