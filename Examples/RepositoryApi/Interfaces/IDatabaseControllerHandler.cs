using Infrastructure.Database.Models.Entities;

namespace RepositoryApi.Interfaces;

public interface IDatabaseControllerHandler
{
    IEnumerable<Person> GetAll();

    Task<Person?> GetAsync(Guid id);

    Task<Guid> AddAsync(Person person);
}