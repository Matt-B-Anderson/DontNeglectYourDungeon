using System.ComponentModel.DataAnnotations;

namespace DontNeglectYourDungeon.Data.Models;

public class Session
{
    public int Id { get; set; }

    [Required]
    public int CampaignId { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = "Session";

    [Required]
    public DateTime ScheduledAtUtc { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string? LocationOrLink { get; set; }

    [StringLength(8000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Campaign? Campaign { get; set; }
}
