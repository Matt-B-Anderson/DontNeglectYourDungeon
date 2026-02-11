using DontNeglectYourDungeon.Data.Models;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Services;

public interface ICharacterLinkService
{
    Task<List<CharacterLink>> GetForCampaignAsync(ClaimsPrincipal user, int campaignId, CancellationToken ct = default);
    Task<CharacterLink?> GetByIdAsync(ClaimsPrincipal user, int characterLinkId, CancellationToken ct = default);

    Task<int?> CreateAsync(ClaimsPrincipal user, int campaignId, string name, string url, CancellationToken ct = default);
    Task<bool> UpdateAsync(ClaimsPrincipal user, int characterLinkId, string name, string url, CancellationToken ct = default);
    Task<bool> DeleteAsync(ClaimsPrincipal user, int characterLinkId, CancellationToken ct = default);
}
