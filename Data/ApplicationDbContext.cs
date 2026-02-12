using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DontNeglectYourDungeon.Data.Models;
using Microsoft.EntityFrameworkCore;
using DontNeglectYourDungeon.Data.Models;

namespace DontNeglectYourDungeon.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Session> Sessions => Set<Session>();
<<<<<<< HEAD
    public DbSet<CharacterLink> CharacterLinks => Set<CharacterLink>();
=======
    public DbSet<Models.Character> Characters => Set<Models.Character>();
    public DbSet<LinkedCharacter> LinkedCharacters => Set<LinkedCharacter>();
>>>>>>> a6185c00b6ddfb6fa05f94aaff8355a184bbb0c2

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Campaign>()
            .HasMany(c => c.Sessions)
            .WithOne(s => s.Campaign!)
            .HasForeignKey(s => s.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Campaign>()
<<<<<<< HEAD
            .HasMany(c => c.CharacterLinks)
            .WithOne(cl => cl.Campaign!)
            .HasForeignKey(cl => cl.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Campaign>()
            .HasIndex(c => new { c.OwnerId, c.Name });
    }

=======
            .HasMany(c => c.Characters)
            .WithOne(ch => ch.Campaign!)
            .HasForeignKey(ch => ch.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Helpful indexes
        builder.Entity<Campaign>()
            .HasIndex(c => c.OwnerUserId);

        builder.Entity<Models.Character>()
            .HasIndex(ch => ch.OwnerUserId);

        builder.Entity<LinkedCharacter>()
            .HasIndex(lc => lc.OwnerUserId);
    }
>>>>>>> a6185c00b6ddfb6fa05f94aaff8355a184bbb0c2
}
