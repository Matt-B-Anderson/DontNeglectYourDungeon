using System.ComponentModel.DataAnnotations;

namespace DontNeglectYourDungeon.Data.Models;

/// <summary>
/// Represents a user's membership in a campaign.
/// Players join campaigns via join code.
/// The campaign owner is automatically a member.
/// </summary>
public class CampaignMember
{
    public int Id { get; set; }

    [Required]
    public int CampaignId { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public DateTime JoinedUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Campaign? Campaign { get; set; }
}
