using Bogus;
using Microsoft.EntityFrameworkCore;
using UniversalRepositoryTests.DbContexts;
using UniversalRepositoryTests.Models.Entities;

namespace UniversalRepositoryTests.Utils;

public class DbService
{
    public static TestContext Initialize()
    {
        var options = new DbContextOptionsBuilder<TestContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new TestContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public static async Task PopulateDatabaseAsync(DbContext context, int entitiesCount)
    {
        var personGenerator = GetPersonGenerator();
        var userRoleGenerator = GetUserRoleGenerator();

        var people = personGenerator.Generate(entitiesCount);
        var userRoles = userRoleGenerator.Generate(entitiesCount);

        await context.AddRangeAsync(people);
        await context.AddRangeAsync(userRoles);
        await context.SaveChangesAsync();
    }

    public static TestPerson GetPerson()
    {
        var personGenerator = GetPersonGenerator();
        return personGenerator.Generate();
    }

    public static TestUserRole GetUserRole()
    {
        var userRoleGenerator = GetUserRoleGenerator();
        return userRoleGenerator.Generate();
    }

    private static Faker<TestUserRole> GetUserRoleGenerator()
    {
        return new Faker<TestUserRole>()
            .RuleFor(ur => ur.UserId, f => f.Random.Guid())
            .RuleFor(ur => ur.RoleId, f => f.Random.Int());
    }

    private static Faker<TestPerson> GetPersonGenerator()
    {
        return new Faker<TestPerson>()
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.DateOfBirth, f => f.Date.Past(30))
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber());
    }
}