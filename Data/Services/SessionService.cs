using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services;

/// <summary>
/// Service for managing game sessions (scheduled D&D sessions/meetings).
///
/// Authorization Model:
/// - View: Any campaign member can view all sessions in their campaign
/// - Create: Any campaign member can create sessions
/// - Edit/Delete: Only the user who created the session can edit/delete it
///
/// This allows both DMs and players to schedule sessions, but prevents others from
/// modifying sessions they didn't create.
/// </summary>
public class SessionService(ApplicationDbContext db) : ISessionService
{
    /// <summary>
    /// Helper method to extract the current user's ID from their authentication claims.
    /// </summary>
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found.");
    }

    /// <summary>
    /// Checks if a user is a member of a specific campaign.
    /// Used for authorization - only campaign members can view/create sessions.
    /// </summary>
    private async Task<bool> UserIsCampaignMember(int campaignId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        return await db.CampaignMembers.AnyAsync(cm => cm.CampaignId == campaignId && cm.UserId == userId);
    }

    /// <summary>
    /// Gets all sessions for a campaign. Only campaign members can view.
    /// Returns empty list if user is not a campaign member.
    /// </summary>
    public async Task<List<Session>> GetForCampaignAsync(int campaignId, ClaimsPrincipal user)
    {
        // Only campaign members can view sessions
        if (!await UserIsCampaignMember(campaignId, user)) return new();

        return await db.Sessions
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.SessionDate)  // Most recent first
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific session by ID. Only campaign members can view.
    /// Returns null if session doesn't exist or user is not a campaign member.
    /// </summary>
    public async Task<Session?> GetByIdAsync(int sessionId, ClaimsPrincipal user)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null) return null;

        // Only campaign members can view this session
        return await UserIsCampaignMember(session.CampaignId, user) ? session : null;
    }

    /// <summary>
    /// Creates a new session. Any campaign member can create.
    /// The creating user becomes the owner and can edit/delete later.
    /// </summary>
    public async Task<Session> CreateAsync(Session session, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);

        // Only campaign members can create sessions
        if (!await UserIsCampaignMember(session.CampaignId, user))
            throw new UnauthorizedAccessException("You are not a member of this campaign.");

        // Track who created this session (for edit/delete permissions)
        session.CreatedByUserId = userId;

        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    /// <summary>
    /// Updates a session. Only the user who created it can edit.
    /// Returns false if session doesn't exist or user is not the creator.
    /// </summary>
    public async Task<bool> UpdateAsync(Session session, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var existing = await db.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        if (existing is null) return false;

        // Only the creator can edit their session (not even the DM can edit others' sessions)
        if (existing.CreatedByUserId != userId) return false;

        // Update all editable fields
        existing.Title = session.Title;
        existing.SessionDate = session.SessionDate;
        existing.Summary = session.Summary;
        existing.NextSteps = session.NextSteps;
        existing.LocationOrLink = session.LocationOrLink;

        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a session. Only the user who created it can delete.
    /// Returns false if session doesn't exist or user is not the creator.
    /// </summary>
    public async Task<bool> DeleteAsync(int sessionId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var existing = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (existing is null) return false;

        // Only the creator can delete their session
        if (existing.CreatedByUserId != userId) return false;

        db.Sessions.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
