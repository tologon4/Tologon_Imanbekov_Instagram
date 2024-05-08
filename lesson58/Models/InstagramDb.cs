using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace lesson58.Models;

public class InstagramDb : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<User> Users { get; set; }
    public DbSet<SubAndSub> SubAndSubs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<UserPostComm> UserPostComms { get; set; }
    public DbSet<UserPostLike> UserPostLikes { get; set; }

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
        
        modelBuilder.Entity<User>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.OwnerUser)
            .HasForeignKey(e => e.OwnerUserId);

        modelBuilder.Entity<UserPostLike>()
            .HasKey(bc => new { bc.UserId, bc.PostId });  
        modelBuilder.Entity<UserPostLike>()
            .HasOne(bc => bc.User)
            .WithMany(b => b.Likes)
            .HasForeignKey(bc => bc.UserId);  
        modelBuilder.Entity<UserPostLike>()
            .HasOne(bc => bc.Post)
            .WithMany(c => c.LikeUsers)
            .HasForeignKey(bc => bc.PostId);
    }
}