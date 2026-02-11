using System.ComponentModel.DataAnnotations;

namespace DontNeglectYourDungeon.Data.Models;

public class Campaign
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(600)]
    public string? Description { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Session> Sessions { get; set; } = new();
    public List<CharacterLink> CharacterLinks { get; set; } = new();
}
