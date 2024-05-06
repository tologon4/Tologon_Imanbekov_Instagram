using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace lesson58.Models;

public class InstagramDb : IdentityDbContext
{
    public DbSet<User> Users { get; set; }
    public InstagramDb(DbContextOptions<InstagramDb> options) : base(options){}

}