using UniRepo.Interfaces;
using UniRepo.Repositories;
using UniversalRepositoryTests.DbContexts;
using UniversalRepositoryTests.Models.Entities;
using UniversalRepositoryTests.Utils;

namespace UniversalRepositoryTests.IntegrationTests;

public class UniversalRepositoryIntegrationTests
{
    private readonly TestContext _context;
    private readonly IUniversalRepository<TestContext, TestPerson> _peopleRepository;
    private readonly IUniversalRepository<TestContext, TestUserRole> _rolesRepository;

    public UniversalRepositoryIntegrationTests()
    {
        _context = DbService.Initialize();
        _peopleRepository = new UniversalRepository<TestContext, TestPerson>(_context);
        _rolesRepository = new UniversalRepository<TestContext, TestUserRole>(_context);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetAll_NotEmptyDatabase_ReturnsAllEntities(bool isReadonly)
    {
        var entitiesCount = 23;

        await DbService.PopulateDatabaseAsync(_context, entitiesCount);

        var resultPeople = _peopleRepository.GetAll(isReadonly).ToArray();
        var resultRoles = _rolesRepository.GetAll(isReadonly).ToArray();

        Assert.NotNull(resultPeople);
        Assert.NotNull(resultRoles);
        Assert.NotEmpty(resultPeople);
        Assert.NotEmpty(resultRoles);
        Assert.Equal(entitiesCount, resultPeople.Length);
        Assert.Equal(entitiesCount, resultRoles.Length);
    }

    [Fact]
    public async Task GetById_EntityExists_ReturnsEntity()
    {
        await DbService.PopulateDatabaseAsync(_context, 5);
        var person = DbService.GetPerson();
        await _context.AddAsync(person);
        await _context.SaveChangesAsync();

        var resultPerson = await _peopleRepository.GetByIdAsync(person.Id);

        Assert.NotNull(resultPerson);
        Assert.Equal(person.Id, resultPerson!.Id);
    }

    [Fact]
    public async Task GetById_EntityDoesNotExist_ReturnsNull()
    {
        var resultPerson = await _peopleRepository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(resultPerson);
    }

    [Fact]
    public async Task GetById_NullId_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _peopleRepository.GetByIdAsync(id: null!));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByCompositeId_EntityExists_ReturnsEntity(bool isReadonly)
    {
        await DbService.PopulateDatabaseAsync(_context, 5);
        var userRole = DbService.GetUserRole();
        await _context.AddAsync(userRole);
        await _context.SaveChangesAsync();

        var resultUserRole = await _rolesRepository.GetByCompositeIdAsync(
            new object[] { userRole.UserId, userRole.RoleId }, isReadonly);

        Assert.NotNull(resultUserRole);
        Assert.Equal(userRole.UserId, resultUserRole!.UserId);
        Assert.Equal(userRole.RoleId, resultUserRole!.RoleId);
    }

    [Fact]
    public async Task GetByCompositeId_EntityDoesNotExist_ReturnsNull()
    {
        var resultUserRole = await _rolesRepository.GetByCompositeIdAsync(
                       new object[] { Guid.NewGuid(), 1 });

        Assert.Null(resultUserRole);
    }

    [Fact]
    public async Task GetByCompositeId_NullId_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _rolesRepository.GetByCompositeIdAsync(keys: null!));
    }

    [Fact]
    public async Task GetByCompositeId_NullIdOneElement_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _rolesRepository.GetByCompositeIdAsync(keys: new object[] { null! }));
    }

    [Fact]
    public async Task GetByCompositeId_NullIdTwoElements_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _rolesRepository.GetByCompositeIdAsync(keys: new object[] { Guid.NewGuid(), null! }));
    }

    [Fact]
    public async Task Create_ValidEntity_AddsEntity()
    {
        var person = DbService.GetPerson();

        var resultId = await _peopleRepository.CreateAsync(person);

        Assert.NotNull(resultId);
        Assert.Equal(person.Id, (Guid)resultId!);

        var resultPerson = await _peopleRepository.GetByIdAsync(person.Id);

        Assert.NotNull(resultPerson);
        Assert.Equal(person.Id, resultPerson!.Id);
    }

    [Fact]
    public async Task Create_NullEntity_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _peopleRepository.CreateAsync(entity: null!));
    }

    [Fact]
    public async Task Update_ValidEntity_UpdatesEntity()
    {
        var updatedFirstName = "UpdatedFirstName";
        var person = DbService.GetPerson();
        await _context.AddAsync(person);
        await _context.SaveChangesAsync();
        person.FirstName = updatedFirstName;

        await _peopleRepository.UpdateAsync(person);

        var resultPerson = await _peopleRepository.GetByIdAsync(person.Id);

        Assert.NotNull(resultPerson);
        Assert.Equal(updatedFirstName, resultPerson!.FirstName);
    }

    [Fact]
    public async Task Update_NullEntity_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _peopleRepository.UpdateAsync(entity: null!));
    }

    [Fact]
    public async Task Patch_ValidEntity_UpdatesEntity()
    {
        var updatedFirstName = "UpdatedFirstName";
        var person = DbService.GetPerson();
        await _context.AddAsync(person);
        await _context.SaveChangesAsync();
        person.FirstName = updatedFirstName;

        await _peopleRepository.PatchAsync(person);

        var resultPerson = await _peopleRepository.GetByIdAsync(person.Id);

        Assert.NotNull(resultPerson);
        Assert.Equal(updatedFirstName, resultPerson!.FirstName);
    }

    [Fact]
    public async Task Patch_NoEntityInDatabase_ThrowsInvalidOperationException()
    {
        var person = DbService.GetPerson();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _peopleRepository.PatchAsync(person));
    }

    [Fact]
    public async Task Patch_NullEntity_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _peopleRepository.PatchAsync(entity: null!));
    }

    [Fact]
    public async Task Delete_EntityExists_DeletesEntity()
    {
        var person = DbService.GetPerson();
        await _context.AddAsync(person);
        await _context.SaveChangesAsync();

        await _peopleRepository.DeleteAsync(person.Id);

        var resultPerson = await _peopleRepository.GetByIdAsync(person.Id);

        Assert.Null(resultPerson);
    }

    [Fact]
    public async Task Delete_EntityDoesNotExist_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _peopleRepository.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Delete_NullId_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _peopleRepository.DeleteAsync(id: null!));
    }
}