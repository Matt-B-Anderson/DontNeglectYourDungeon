using System.ComponentModel.DataAnnotations;

namespace DontNeglectYourDungeon.Data.Models;

/// <summary>
/// A Campaign is owned by one user (usually the DM).
/// UI/Auth note: Only the owner should be allowed to edit/delete.
/// </summary>
public class Campaign
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = "";

    [StringLength(40)]
    public string? System { get; set; } // "D&D 5e", "Pathfinder", etc.

    [StringLength(1000)]
    public string? Description { get; set; }

    // Auth note: store the logged-in user's ID here :)
    [Required]
    public string OwnerUserId { get; set; } = "";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [Required, StringLength(8)]
    public string JoinCode { get; set; } = "";

    // Navigation
    public List<Session> Sessions { get; set; } = new();
    public List<Character> Characters { get; set; } = new();
    public List<LinkedCharacter> LinkedCharacters { get; set; } = new();
}
