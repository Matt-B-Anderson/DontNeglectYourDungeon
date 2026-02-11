using DontNeglectYourDungeon.Data.Models;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Services;

public interface ICampaignService
{
    Task<List<Campaign>> GetMineAsync(ClaimsPrincipal user, CancellationToken ct = default);
    Task<Campaign?> GetMineByIdAsync(ClaimsPrincipal user, int campaignId, CancellationToken ct = default);
    Task<int> CreateAsync(ClaimsPrincipal user, string name, string? description, CancellationToken ct = default);
    Task<bool> UpdateAsync(ClaimsPrincipal user, int campaignId, string name, string? description, CancellationToken ct = default);
    Task<bool> DeleteAsync(ClaimsPrincipal user, int campaignId, CancellationToken ct = default);
}
