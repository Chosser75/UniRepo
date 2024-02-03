using Bogus;
using Infrastructure.Database.DbContexts;
using Infrastructure.Database.Interfaces;

namespace Infrastructure.Database.Services;

public class DbService : IDbService
{
    private readonly UniRepoContext _context;

    public DbService(UniRepoContext context)
    {
        _context = context;
    }

    public async Task PopulateDatabaseAsync()
    {
        var personGenerator = new Faker<Models.Entities.Person>()
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.DateOfBirth, f => f.Date.Past(30))
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber());

        var userRoleGenerator = new Faker<Models.Entities.UserRole>()
            .RuleFor(ur => ur.UserId, f => f.Random.Guid())
            .RuleFor(ur => ur.RoleId, f => f.Random.Guid());

        // Generate a list of entities
        var people = personGenerator.Generate(100);
        var userRoles = userRoleGenerator.Generate(10);

        await _context.AddRangeAsync(people);
        await _context.AddRangeAsync(userRoles);
        await _context.SaveChangesAsync();
    }
}