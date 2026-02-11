using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DontNeglectYourDungeon.Data.Models;

namespace DontNeglectYourDungeon.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Models.Character> Characters => Set<Models.Character>();
    public DbSet<LinkedCharacter> LinkedCharacters => Set<LinkedCharacter>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Campaign>()
            .HasMany(c => c.Sessions)
            .WithOne(s => s.Campaign!)
            .HasForeignKey(s => s.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Campaign>()
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
}
