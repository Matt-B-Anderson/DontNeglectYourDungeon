using System.ComponentModel.DataAnnotations;

namespace DontNeglectYourDungeon.Data.Models;

public enum CharacterStatus
{
    Active = 0,
    Retired = 1,
    Dead = 2
}

/// <summary>
/// A Character belongs to a Campaign and has an OwnerUserId (the player).
/// Auth note: only the Owner can edit/delete their character.
/// UI note: campaign owner can still view all characters for the campaign.
/// </summary>
public class Character
{
    public int Id { get; set; }

    [Required]
    public int CampaignId { get; set; }

    [Required]
    public string OwnerUserId { get; set; } = "";

    [Required, StringLength(60)]
    public string Name { get; set; } = "";

    [StringLength(60)]
    public string? Class { get; set; }

    [Range(1, 20)]
    public int Level { get; set; } = 1;

    public CharacterStatus Status { get; set; } = CharacterStatus.Active;

    [StringLength(2000)]
    public string? Notes { get; set; }

    // Navigation
    public Campaign? Campaign { get; set; }
}
