using Infrastructure.Database.DbContexts;
using Infrastructure.Database.Interfaces;
using Infrastructure.Database.Models.Entities;
using RepositoryApi.Interfaces;
using UniRepo.Interfaces;

namespace RepositoryApi.ControllerHandlers;

public class PeopleControllerHandler : IDatabaseControllerHandler
{
    private readonly IUniversalRepository<UniRepoContext, Person, Guid> _repository;
    private readonly IDbService _dbService;

    public PeopleControllerHandler(
        IUniversalRepository<UniRepoContext, Person, Guid> repository,
        IDbService dbService)
    {
        _repository = repository;
        _dbService = dbService;
    }

    public IEnumerable<Person> GetAll() => _repository.GetAll();

    public async Task<Person?> GetAsync(Guid id) => await _repository.GetByIdAsync(id);

    public async Task<Guid> AddAsync(Person person) => await _repository.CreateAsync(person);
}