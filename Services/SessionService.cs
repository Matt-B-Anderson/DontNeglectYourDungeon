using DontNeglectYourDungeon.Data;
using DontNeglectYourDungeon.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Services;

public sealed class SessionService : ISessionService
{
    private readonly ApplicationDbContext _db;

    public SessionService(ApplicationDbContext db)
    {
        _db = db;
    }

    private static string? GetUserId(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    private static DateTime LocalToUtc(DateTime localDateTime)
        => DateTime.SpecifyKind(localDateTime, DateTimeKind.Local).ToUniversalTime();

    public async Task<List<Session>> GetForCampaignAsync(ClaimsPrincipal user, int campaignId, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return [];

        var owns = await _db.Campaigns.AsNoTracking()
            .AnyAsync(c => c.Id == campaignId && c.OwnerId == userId, ct);

        if (!owns)
            return [];

        return await _db.Sessions.AsNoTracking()
            .Where(s => s.CampaignId == campaignId)
            .OrderBy(s => s.ScheduledAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Session?> GetByIdAsync(ClaimsPrincipal user, int sessionId, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return await _db.Sessions
            .Include(s => s.Campaign)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Campaign!.OwnerId == userId, ct);
    }

    public async Task<int?> CreateAsync(
        ClaimsPrincipal user,
        int campaignId,
        string title,
        DateTime scheduledLocal,
        string? locationOrLink,
        string? notes,
        CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var campaign = await _db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.OwnerId == userId, ct);
        if (campaign is null)
            return null;

        var nowUtc = DateTime.UtcNow;

        var session = new Session
        {
            CampaignId = campaignId,
            Title = title.Trim(),
            ScheduledAtUtc = LocalToUtc(scheduledLocal),
            LocationOrLink = string.IsNullOrWhiteSpace(locationOrLink) ? null : locationOrLink.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };

        _db.Sessions.Add(session);
        campaign.UpdatedAtUtc = nowUtc;

        await _db.SaveChangesAsync(ct);
        return session.Id;
    }

    public async Task<bool> UpdateAsync(
        ClaimsPrincipal user,
        int sessionId,
        string title,
        DateTime scheduledLocal,
        string? locationOrLink,
        CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var session = await _db.Sessions
            .Include(s => s.Campaign)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Campaign!.OwnerId == userId, ct);

        if (session is null)
            return false;

        var nowUtc = DateTime.UtcNow;

        session.Title = title.Trim();
        session.LocationOrLink = string.IsNullOrWhiteSpace(locationOrLink) ? null : locationOrLink.Trim();
        session.ScheduledAtUtc = LocalToUtc(scheduledLocal);
        session.UpdatedAtUtc = nowUtc;

        session.Campaign!.UpdatedAtUtc = nowUtc;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateNotesAsync(ClaimsPrincipal user, int sessionId, string? notes, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var session = await _db.Sessions
            .Include(s => s.Campaign)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Campaign!.OwnerId == userId, ct);

        if (session is null)
            return false;

        var nowUtc = DateTime.UtcNow;

        session.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        session.UpdatedAtUtc = nowUtc;
        session.Campaign!.UpdatedAtUtc = nowUtc;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(ClaimsPrincipal user, int sessionId, CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var session = await _db.Sessions
            .Include(s => s.Campaign)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Campaign!.OwnerId == userId, ct);

        if (session is null)
            return false;

        var nowUtc = DateTime.UtcNow;

        _db.Sessions.Remove(session);
        session.Campaign!.UpdatedAtUtc = nowUtc;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
