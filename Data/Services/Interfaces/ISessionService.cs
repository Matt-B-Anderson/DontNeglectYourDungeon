using DontNeglectYourDungeon.Data.Models;

namespace DontNeglectYourDungeon.Data.Services.Interfaces;

public interface ISessionService
{
    Task<List<Session>> GetForCampaignAsync(int campaignId, string currentUserId);
    Task<Session?> GetByIdAsync(int sessionId, string currentUserId);

    Task<Session> CreateAsync(Session session, string currentUserId);
    Task<bool> UpdateAsync(Session session, string currentUserId);
    Task<bool> DeleteAsync(int sessionId, string currentUserId);
}
