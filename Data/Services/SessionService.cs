using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DontNeglectYourDungeon.Data.Services;

public class SessionService(ApplicationDbContext db) : ISessionService
{
    private async Task<bool> CampaignOwnedByUser(int campaignId, string userId)
        => await db.Campaigns.AnyAsync(c => c.Id == campaignId && c.OwnerUserId == userId);

    public async Task<List<Session>> GetForCampaignAsync(int campaignId, string currentUserId)
    {
        if (!await CampaignOwnedByUser(campaignId, currentUserId)) return new();

        return await db.Sessions
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<Session?> GetByIdAsync(int sessionId, string currentUserId)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null) return null;

        return await CampaignOwnedByUser(session.CampaignId, currentUserId) ? session : null;
    }

    public async Task<Session> CreateAsync(Session session, string currentUserId)
    {
        if (!await CampaignOwnedByUser(session.CampaignId, currentUserId))
            throw new UnauthorizedAccessException("You do not own this campaign.");

        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    public async Task<bool> UpdateAsync(Session session, string currentUserId)
    {
        var existing = await db.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        if (existing is null) return false;

        if (!await CampaignOwnedByUser(existing.CampaignId, currentUserId)) return false;

        existing.Title = session.Title;
        existing.SessionDate = session.SessionDate;
        existing.Summary = session.Summary;
        existing.NextSteps = session.NextSteps;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int sessionId, string currentUserId)
    {
        var existing = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (existing is null) return false;

        if (!await CampaignOwnedByUser(existing.CampaignId, currentUserId)) return false;

        db.Sessions.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
