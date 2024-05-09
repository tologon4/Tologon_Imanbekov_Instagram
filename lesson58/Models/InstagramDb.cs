using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace lesson58.Models;

public class InstagramDb : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<User> Users { get; set; }
    public DbSet<SubAndSub> SubAndSubs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<UserPostLike> UserPostLikes { get; set; }
    public DbSet<UserPostComm> UserPostComms { get; set; }

    public InstagramDb(DbContextOptions<InstagramDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>()
            .HasMany<SubAndSub>(subTo => subTo.Followings)
            .WithOne(u => u.FollowFrom)
            .HasForeignKey(s => s.FollowFromId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<User>()
            .HasMany<SubAndSub>(subFrom => subFrom.Followers)
            .WithOne(u => u.FollowTo)
            .HasForeignKey(s => s.FollowToId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<User>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.OwnerUser)
            .HasForeignKey(e => e.OwnerUserId);

        modelBuilder.Entity<UserPostLike>()
            .HasKey(upl => new { upl.UserId, upl.PostId });

        modelBuilder.Entity<UserPostLike>()
            .HasOne(upl => upl.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(upl => upl.UserId);

        modelBuilder.Entity<UserPostLike>()
            .HasOne(upl => upl.Post)
            .WithMany(p => p.LikeUsers)
            .HasForeignKey(upl => upl.PostId);
        
        modelBuilder.Entity<UserPostComm>()
            .HasIndex(upl => new { upl.UserId, upl.PostId })
            .IsUnique(false);

        modelBuilder.Entity<UserPostComm>()
            .HasOne(upl => upl.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(upl => upl.UserId);

        modelBuilder.Entity<UserPostComm>()
            .HasOne(upl => upl.Post)
            .WithMany(p => p.CommentUsers)
            .HasForeignKey(upl => upl.PostId);
    }
}