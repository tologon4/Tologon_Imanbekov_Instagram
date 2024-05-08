using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace lesson58.Models;

public class InstagramDb : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<User> Users { get; set; }
    public DbSet<SubAndSub> SubAndSubs { get; set; }

    public InstagramDb(DbContextOptions<InstagramDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>()
            .HasMany<SubAndSub>(subTo => subTo.Subscribtions)
            .WithOne(u => u.Subcribtion)
            .HasForeignKey(s => s.SubcribtionId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<User>()
            .HasMany<SubAndSub>(subFrom => subFrom.Subscribers)
            .WithOne(u => u.Subcriber)
            .HasForeignKey(s => s.SubcriberId)
            .OnDelete(DeleteBehavior.NoAction);


    }
}