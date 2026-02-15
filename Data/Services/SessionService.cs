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
    private async Task<bool> UserIsCampaignMember(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        return await db.CampaignMembers.AnyAsync(cm => cm.CampaignId == campaignId && cm.UserId == userId);
    }


    public async Task<List<Session>> GetForCampaignAsync(int campaignId, ClaimsPrincipal user)
    {
        // Allow any campaign member to view sessions
        if (!await UserIsCampaignMember(campaignId, user)) return new();

        return await db.Sessions
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<Session?> GetByIdAsync(int sessionId, ClaimsPrincipal user)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null) return null;

        // Allow any campaign member to view sessions
        return await UserIsCampaignMember(session.CampaignId, user) ? session : null;
    }

    public async Task<Session> CreateAsync(Session session, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);

        // Allow any campaign member to create sessions
        if (!await UserIsCampaignMember(session.CampaignId, user))
            throw new UnauthorizedAccessException("You are not a member of this campaign.");

        session.CreatedByUserId = userId;

        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    public async Task<bool> UpdateAsync(Session session, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var existing = await db.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        if (existing is null) return false;

        // Only allow the creator to edit their session
        if (existing.CreatedByUserId != userId) return false;

        existing.Title = session.Title;
        existing.SessionDate = session.SessionDate;
        existing.Summary = session.Summary;
        existing.NextSteps = session.NextSteps;
        existing.LocationOrLink = session.LocationOrLink;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int sessionId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var existing = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (existing is null) return false;

        // Only allow the creator to delete their session
        if (existing.CreatedByUserId != userId) return false;

        db.Sessions.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
