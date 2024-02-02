using Infrastructure.Database.Models.Entities;

namespace RepositoryApi.Interfaces;

public interface IDatabaseControllerHandler
{
    IEnumerable<Person> GetPeople();
}