using Infrastructure.Database.DbContexts;
using Infrastructure.Database.Models.Entities;
using RepositoryApi.Interfaces;
using UniRepo.Interfaces;

namespace RepositoryApi.ControllerHandlers;

public class PeopleControllerHandler : IPeopleControllerHandler
{
    private readonly IUniversalRepository<UniRepoContext, Person> _repository;

    public PeopleControllerHandler(
        IUniversalRepository<UniRepoContext, Person> repository)
    {
        _repository = repository;
    }

    public IEnumerable<Person> GetAll() => _repository.GetAll();

    public async Task<Person?> GetAsync(Guid id) => await _repository.GetByIdAsync(id);

    public async Task<Person?> GetAsync(IEnumerable<Guid> keys) => await _repository.GetByIdAsync(keys);

    public async Task<Person> CreateAsync(Person person) => await _repository.CreateAsync(person);

    public async Task UpdateAsync(Person person) => await _repository.UpdateAsync(person);

    public async Task UpdateModifiedFieldsAsync(Person person) => await _repository.PatchAsync(person);

    public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

    public async Task<string> GetFirstNameAsync(Guid id)
    {
        var personProjection = await _repository.GetProjectionAsync(
            p => new { p.FirstName }, p => p.Id == id);

        return personProjection?.FirstName ?? string.Empty;
    }

    public async Task<IEnumerable<Person>> GetYoungerAsync(DateTime date)
    {
        Func<IQueryable<Person>, IQueryable<Person>> queryShaper =
            q => q.Where(a => a.DateOfBirth > date);

        return await _repository.QueryCollectionAsync(queryShaper);
    }

    public async Task<Person?> GetByFirstNameAsync(string firstName)
    {
        Func<IQueryable<Person>, IQueryable<Person>> queryShaper =
            q => q.Where(a => a.FirstName.Equals(firstName));

        return await _repository.QuerySingleAsync(queryShaper);
    }

    public async Task<IEnumerable<string>> GetYoungerNamesAsync(DateTime date)
    {
        var peopleProjections = await _repository.GetProjectionsAsync(
            p => new { p.FirstName, p.LastName },
            p => p.DateOfBirth > date);

        return peopleProjections.Select(p => $"{p.FirstName} {p.LastName}");
    }
}