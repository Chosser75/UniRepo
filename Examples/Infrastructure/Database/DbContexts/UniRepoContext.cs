using Infrastructure.Database.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.DbContexts;

public class UniRepoContext : DbContext
{
    public UniRepoContext(DbContextOptions<UniRepoContext> options) : base(options)
    {
    }

    public DbSet<Person> People { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>()
            .HasKey(e => new { e.UserId, e.RoleId });
    }
}