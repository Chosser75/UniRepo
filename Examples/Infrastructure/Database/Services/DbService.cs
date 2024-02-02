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
        var userGenerator = new Faker<Models.Entities.Person>()
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.DateOfBirth, f => f.Date.Past(30))
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber());

        // Generate a list of entities
        var people = userGenerator.Generate(100);

        await _context.AddRangeAsync(people);
        await _context.SaveChangesAsync();
    }
}