using Microsoft.EntityFrameworkCore;
using UniversalRepositoryTests.Models.Entities;

namespace UniversalRepositoryTests.DbContexts;

public class TestContext : DbContext
{
    public TestContext(DbContextOptions<TestContext> options) : base(options)
    {
    }

    public DbSet<TestPerson> People { get; set; }

    public DbSet<TestUserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestUserRole>()
            .HasKey(e => new { e.UserId, e.RoleId });
    }
}