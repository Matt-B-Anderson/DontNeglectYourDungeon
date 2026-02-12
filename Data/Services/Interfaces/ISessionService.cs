using DontNeglectYourDungeon.Data.Models;
using System.Security.Claims;

namespace DontNeglectYourDungeon.Data.Services.Interfaces;

public interface ISessionService
{
    Task<List<Session>> GetForCampaignAsync(int campaignId, ClaimsPrincipal user);
    Task<Session?> GetByIdAsync(int sessionId, ClaimsPrincipal user);

    Task<Session> CreateAsync(Session session, ClaimsPrincipal user);
    Task<bool> UpdateAsync(Session session, ClaimsPrincipal user);
    Task<bool> DeleteAsync(int sessionId, ClaimsPrincipal user);
}
