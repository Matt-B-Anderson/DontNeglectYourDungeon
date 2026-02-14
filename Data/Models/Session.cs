using System.ComponentModel.DataAnnotations;

namespace DontNeglectYourDungeon.Data.Models;

/// <summary>
/// A Session belongs to a Campaign.
/// UI note: create/edit sessions from within a Campaign context.
/// Auth note: only campaign owner manages sessions.
/// </summary>
public class Session
{
    public int Id { get; set; }

    [Required]
    public int CampaignId { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [Required]
    public DateTime SessionDate { get; set; }

    [StringLength(2000)]
    public string? Summary { get; set; }

    [StringLength(2000)]
    public string? NextSteps { get; set; }

    [StringLength(200)]
    public string? LocationOrLink { get; set; }

    // Navigation
    public Campaign? Campaign { get; set; }
}
