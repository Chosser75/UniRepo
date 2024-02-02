using Infrastructure.Database.DbContexts;
using Infrastructure.Database.Models.Entities;
using RepositoryApi.Interfaces;
using UniRepo.Interfaces;

namespace RepositoryApi.ControllerHandlers;

public class PeopleControllerHandler : IPeopleControllerHandler
{
    private readonly IUniversalRepository<UniRepoContext, Person, Guid> _repository;

    public PeopleControllerHandler(
        IUniversalRepository<UniRepoContext, Person, Guid> repository)
    {
        _repository = repository;
    }

    public IEnumerable<Person> GetAll() => _repository.GetAll();

    public async Task<Person?> GetAsync(Guid id) => await _repository.GetByIdAsync(id);

    public async Task<Guid> AddAsync(Person person) => await _repository.CreateAsync(person);

    public async Task UpdateAsync(Person person) => await _repository.UpdateAsync(person);

    public async Task UpdateModifiedFieldsAsync(Person person) => await _repository.UpdateModifiedPropertiesAsync(person);

    public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

    public async Task<string> GetFirstNameAsync(Guid id)
    {
        var personProjection = await _repository.GetProjectionAsync(p => new { p.FirstName }, id);

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
}