using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services;

public class SessionService(ApplicationDbContext db) : ISessionService
{

    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found.");
    }
    private async Task<bool> CampaignOwnedByUser(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        return await db.Campaigns.AnyAsync(c => c.Id == campaignId && c.OwnerUserId == userId);
    }


    public async Task<List<Session>> GetForCampaignAsync(int campaignId, ClaimsPrincipal user)
    {
        if (!await CampaignOwnedByUser(campaignId, user)) return new();

        return await db.Sessions
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<Session?> GetByIdAsync(int sessionId, ClaimsPrincipal user)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null) return null;

        return await CampaignOwnedByUser(session.CampaignId, user) ? session : null;
    }

    public async Task<Session> CreateAsync(Session session, ClaimsPrincipal user)
    {
        if (!await CampaignOwnedByUser(session.CampaignId, user))
            throw new UnauthorizedAccessException("You do not own this campaign.");

        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    public async Task<bool> UpdateAsync(Session session, ClaimsPrincipal user)
    {
        var existing = await db.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        if (existing is null) return false;

        if (!await CampaignOwnedByUser(existing.CampaignId, user)) return false;

        existing.Title = session.Title;
        existing.SessionDate = session.SessionDate;
        existing.Summary = session.Summary;
        existing.NextSteps = session.NextSteps;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int sessionId, ClaimsPrincipal user)
    {
        var existing = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (existing is null) return false;

        if (!await CampaignOwnedByUser(existing.CampaignId, user)) return false;

        db.Sessions.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
