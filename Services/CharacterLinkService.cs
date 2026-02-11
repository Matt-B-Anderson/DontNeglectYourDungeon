using DontNeglectYourDungeon.Data;
using DontNeglectYourDungeon.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Services;

public sealed class CharacterLinkService : ICharacterLinkService
{
    private readonly ApplicationDbContext _db;

    public CharacterLinkService(ApplicationDbContext db)
    {
        _db = db;
    }

    private static string? GetUserId(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool IsValidHttpUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var u)
           && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);

    public async Task<List<CharacterLink>> GetForCampaignAsync(ClaimsPrincipal user, int campaignId, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return [];

        var owns = await _db.Campaigns.AsNoTracking()
            .AnyAsync(c => c.Id == campaignId && c.OwnerId == userId, ct);

        if (!owns)
            return [];

        return await _db.CharacterLinks.AsNoTracking()
            .Where(cl => cl.CampaignId == campaignId)
            .OrderBy(cl => cl.Name)
            .ToListAsync(ct);
    }

    public async Task<CharacterLink?> GetByIdAsync(ClaimsPrincipal user, int characterLinkId, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return await _db.CharacterLinks
            .Include(cl => cl.Campaign)
            .FirstOrDefaultAsync(cl => cl.Id == characterLinkId && cl.Campaign!.OwnerId == userId, ct);
    }

    public async Task<int?> CreateAsync(ClaimsPrincipal user, int campaignId, string name, string url, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var campaign = await _db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerId == userId, ct);
        if (campaign is null)
            return null;

        url = url.Trim();
        if (!IsValidHttpUrl(url))
            throw new ArgumentException("Please enter a valid http/https URL.");

        var nowUtc = DateTime.UtcNow;

        var link = new CharacterLink
        {
            CampaignId = campaignId,
            Name = name.Trim(),
            Url = url,
            CreatedAt = nowUtc
        };

        _db.CharacterLinks.Add(link);

        campaign.UpdatedAtUtc = nowUtc;

        await _db.SaveChangesAsync(ct);
        return link.Id;
    }

    public async Task<bool> UpdateAsync(ClaimsPrincipal user, int characterLinkId, string name, string url, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var link = await _db.CharacterLinks
            .Include(cl => cl.Campaign)
            .FirstOrDefaultAsync(cl => cl.Id == characterLinkId && cl.Campaign!.OwnerId == userId, ct);

        if (link is null)
            return false;

        url = url.Trim();
        if (!IsValidHttpUrl(url))
            throw new ArgumentException("Please enter a valid http/https URL.");

        var nowUtc = DateTime.UtcNow;

        link.Name = name.Trim();
        link.Url = url;
        link.Campaign!.UpdatedAtUtc = nowUtc;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(ClaimsPrincipal user, int characterLinkId, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var link = await _db.CharacterLinks
            .Include(cl => cl.Campaign)
            .FirstOrDefaultAsync(cl => cl.Id == characterLinkId && cl.Campaign!.OwnerId == userId, ct);

        if (link is null)
            return false;

        link.Campaign!.UpdatedAtUtc = DateTime.UtcNow;

        _db.CharacterLinks.Remove(link);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
