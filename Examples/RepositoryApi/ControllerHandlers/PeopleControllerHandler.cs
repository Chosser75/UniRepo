using Infrastructure.Database.DbContexts;
using Infrastructure.Database.Models.Entities;
using RepositoryApi.Interfaces;
using UniRepo.Interfaces;

namespace RepositoryApi.ControllerHandlers;

public class PeopleControllerHandler : IDatabaseControllerHandler
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
}