using Infrastructure.Database.Models.Entities;

namespace RepositoryApi.Interfaces;

public interface IPeopleControllerHandler
{
    IEnumerable<Person> GetAll();

    Task<Person?> GetAsync(Guid id);

    Task<Person?> GetAsync(IEnumerable<Guid> keys);

    Task<Person> CreateAsync(Person person);

    Task UpdateAsync(Person person);

    Task UpdateModifiedFieldsAsync(Person person);

    Task DeleteAsync(Guid id);

    Task<string> GetFirstNameAsync(Guid id);

    Task<IEnumerable<Person>> GetYoungerAsync(DateTime date);

    Task<Person?> GetByFirstNameAsync(string firstName);

    Task<IEnumerable<string>> GetYoungerNamesAsync(DateTime date);
}