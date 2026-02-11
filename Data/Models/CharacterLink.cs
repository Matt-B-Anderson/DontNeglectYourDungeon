using System.ComponentModel.DataAnnotations;

namespace DontNeglectYourDungeon.Data.Models;

public class CharacterLink
{
    public int Id { get; set; }

    [Required]
    public int CampaignId { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, Url, StringLength(500)]
    public string Url { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Campaign? Campaign { get; set; }
}
