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
    public DateTimeOffset ScheduledAt { get; set; } = DateTimeOffset.UtcNow;

    [StringLength(200)]
    public string? LocationOrLink { get; set; }

    [StringLength(8000)]
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Campaign? Campaign { get; set; }
}
