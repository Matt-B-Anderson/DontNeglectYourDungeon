using DontNeglectYourDungeon.Data.Models;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Services;

public interface ISessionService
{
    Task<List<Session>> GetForCampaignAsync(
        ClaimsPrincipal user,
        int campaignId,
        CancellationToken ct = default);

    Task<Session?> GetByIdAsync(
        ClaimsPrincipal user,
        int sessionId,
        CancellationToken ct = default);

    Task<int?> CreateAsync(
        ClaimsPrincipal user,
        int campaignId,
        string title,
        DateTime scheduledLocal,
        string? locationOrLink,
        string? notes,
        CancellationToken ct = default);

    Task<bool> UpdateAsync(
        ClaimsPrincipal user,
        int sessionId,
        string title,
        DateTime scheduledLocal,
        string? locationOrLink,
        CancellationToken ct = default);

    Task<bool> UpdateNotesAsync(
        ClaimsPrincipal user,
        int sessionId,
        string? notes,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        ClaimsPrincipal user,
        int sessionId,
        CancellationToken ct = default);
}
