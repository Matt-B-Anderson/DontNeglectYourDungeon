using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DontNeglectYourDungeon.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DontNeglectYourDungeon.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<CharacterLink> CharacterLinks => Set<CharacterLink>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Campaign>()
            .HasMany(c => c.Sessions)
            .WithOne(s => s.Campaign!)
            .HasForeignKey(s => s.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Campaign>()
            .HasMany(c => c.CharacterLinks)
            .WithOne(cl => cl.Campaign!)
            .HasForeignKey(cl => cl.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Campaign>()
            .HasIndex(c => new { c.OwnerId, c.Name });
    }

}
