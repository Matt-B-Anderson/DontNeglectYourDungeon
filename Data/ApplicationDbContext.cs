using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DontNeglectYourDungeon.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DontNeglectYourDungeon.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Models.Character> Characters => Set<Models.Character>();
    public DbSet<LinkedCharacter> LinkedCharacters => Set<LinkedCharacter>();
    public DbSet<CampaignMember> CampaignMembers => Set<CampaignMember>();

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

        builder.Entity<Campaign>()
            .HasMany(c => c.LinkedCharacters)
            .WithOne(lc => lc.Campaign!)
            .HasForeignKey(lc => lc.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Helpful indexes
        builder.Entity<Campaign>()
            .HasIndex(c => c.OwnerUserId);

        builder.Entity<Campaign>()
            .HasIndex(c => c.JoinCode)
            .IsUnique();

        builder.Entity<CampaignMember>()
            .HasIndex(cm => cm.CampaignId);

        builder.Entity<CampaignMember>()
            .HasIndex(cm => cm.UserId);

        builder.Entity<CampaignMember>()
            .HasIndex(cm => new { cm.CampaignId, cm.UserId })
            .IsUnique();

        builder.Entity<Models.Character>()
            .HasIndex(ch => ch.OwnerUserId);

        builder.Entity<LinkedCharacter>()
            .HasIndex(lc => lc.OwnerUserId);

        builder.Entity<LinkedCharacter>()
            .HasIndex(lc => lc.CampaignId);

        builder.Entity<Session>()
            .HasIndex(s => s.CreatedByUserId);
    }
}
