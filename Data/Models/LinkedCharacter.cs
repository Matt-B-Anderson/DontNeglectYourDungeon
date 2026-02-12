using System.ComponentModel.DataAnnotations;

namespace DontNeglectYourDungeon.Data.Models;

/// <summary>
/// Stores an external D&D Beyond character URL.
/// IMPORTANT: We do NOT pull data from D&D Beyond. This is just a saved link.
/// UI note: open Url in a new tab (a banner would be super cool if possible).
/// </summary>
public class LinkedCharacter
{
    public int Id { get; set; }

    [Required]
    public string OwnerUserId { get; set; } = "";

    [Required, StringLength(80)]
    public string DisplayName { get; set; } = "";

    [Required, StringLength(500)]
    public string Url { get; set; } = "";
}
