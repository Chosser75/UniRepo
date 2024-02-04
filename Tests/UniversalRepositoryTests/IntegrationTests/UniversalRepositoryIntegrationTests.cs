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
    public async Task GetById_EntityWithSingleKeyExists_ReturnsEntity()
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
    public async Task GetById_EntityWithCompositeKeyExists_ReturnsEntity()
    {
        await DbService.PopulateDatabaseAsync(_context, 5);
        var userRole = DbService.GetUserRole();
        await _context.AddAsync(userRole);
        await _context.SaveChangesAsync();

        var resultUserRole = await _rolesRepository.GetByIdAsync(new object[] { userRole.UserId, userRole.RoleId });

        Assert.NotNull(resultUserRole);
        Assert.Equal(userRole.UserId, resultUserRole!.UserId);
        Assert.Equal(userRole.RoleId, resultUserRole!.RoleId);
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
    public async Task GetByCompositeId_EntityWithCompositeKeyExists_ReturnsEntity(bool isReadonly)
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByCompositeId_EntityWithSingleKeyExists_ReturnsEntity(bool isReadonly)
    {
        await DbService.PopulateDatabaseAsync(_context, 5);
        var person = DbService.GetPerson();
        await _context.AddAsync(person);
        await _context.SaveChangesAsync();

        var resultPerson = await _peopleRepository.GetByCompositeIdAsync(
            new object[] { person.Id }, isReadonly);

        Assert.NotNull(resultPerson);
        Assert.Equal(person.Id, resultPerson!.Id);
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

        var returnPerson = await _peopleRepository.CreateAsync(person);

        Assert.NotNull(returnPerson);
        Assert.Equal(person, returnPerson);

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
    public async Task Patch_ValidSungleKeyEntity_UpdatesEntity()
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
    public async Task Patch_ValidCompositeKeyEntity_UpdatesEntity()
    {
        var updatedRoleName = Guid.NewGuid().ToString();
        var userRole = DbService.GetUserRole();
        await _context.AddAsync(userRole);
        await _context.SaveChangesAsync();
        userRole.RoleName = updatedRoleName;

        await _rolesRepository.PatchAsync(userRole);

        var resultRole = await _rolesRepository.GetByCompositeIdAsync(new object[] { userRole.UserId, userRole.RoleId });

        Assert.NotNull(resultRole);
        Assert.Equal(updatedRoleName, resultRole!.RoleName);
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
    public async Task Delete_EntityWithSingleKeyExists_DeletesEntity()
    {
        var person = DbService.GetPerson();
        await _context.AddAsync(person);
        await _context.SaveChangesAsync();

        await _peopleRepository.DeleteAsync(person.Id);

        var resultPerson = await _peopleRepository.GetByIdAsync(person.Id);

        Assert.Null(resultPerson);
    }

    [Fact]
    public async Task Delete_EntityWithCompositeKeyExists_DeletesEntity()
    {
        var userRole = DbService.GetUserRole();
        await _context.AddAsync(userRole);
        await _context.SaveChangesAsync();
        var userRoleFromDatabase = await _rolesRepository.GetByCompositeIdAsync(
            new object[] { userRole.UserId, userRole.RoleId });
        Assert.NotNull(userRoleFromDatabase);

        await _rolesRepository.DeleteAsync(new object[] { userRole.UserId, userRole.RoleId });

        var resultUserRole = await _rolesRepository.GetByCompositeIdAsync(
            new object[] { userRole.UserId, userRole.RoleId });

        Assert.Null(resultUserRole);
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

    [Fact]
    public async Task QuerySingleAsync_EntityExists_ReturnsEntity()
    {
        var firstName = "TestFirstName";
        var lastName = "TestLastName";
        await DbService.PopulateDatabaseAsync(_context, 5);
        var person = DbService.GetPerson();
        person.FirstName = firstName;
        person.LastName = lastName;
        await _context.AddAsync(person);
        await _context.SaveChangesAsync();

        Func<IQueryable<TestPerson>, IQueryable<TestPerson>> queryShaper =
            q => q.Where(a => a.FirstName.Equals(firstName) && a.LastName.Equals(lastName));

        var resultPerson = await _peopleRepository.QuerySingleAsync(queryShaper);

        Assert.NotNull(resultPerson);
        Assert.Equal(person.Id, resultPerson!.Id);
    }

    [Fact]
    public async Task QuerySingleAsync_EntityDoesNotExists_ReturnsEntity()
    {
        var firstName = "TestFirstName";
        var lastName = "TestLastName";

        Func<IQueryable<TestPerson>, IQueryable<TestPerson>> queryShaper =
            q => q.Where(a => a.FirstName.Equals(firstName) && a.LastName.Equals(lastName));

        var resultPerson = await _peopleRepository.QuerySingleAsync(queryShaper);

        Assert.Null(resultPerson);
    }

    [Fact]
    public async Task QueryCollectionAsync_EntitiesExist_ReturnsEntities()
    {
        var firstName = "TestFirstName";
        var lastName = "TestLastName";
        await DbService.PopulateDatabaseAsync(_context, 5);
        var person1 = DbService.GetPerson();
        person1.FirstName = firstName;
        person1.LastName = lastName;
        var person2 = DbService.GetPerson();
        person2.FirstName = firstName;
        person2.LastName = lastName;
        await _context.AddRangeAsync(person1, person2);
        await _context.SaveChangesAsync();

        Func<IQueryable<TestPerson>, IQueryable<TestPerson>> queryShaper =
            q => q.Where(a => a.FirstName.Equals(firstName) && a.LastName.Equals(lastName));

        var resultIds = (await _peopleRepository.QueryCollectionAsync(queryShaper)).Select(p => p.Id).ToList();

        Assert.NotNull(resultIds);
        Assert.Equal(2, resultIds!.Count);
        Assert.Contains(person1.Id, resultIds);
        Assert.Contains(person2.Id, resultIds);
    }

    [Fact]
    public async Task QueryCollectionAsync_EntityDoesNotExist_ReturnsEmptyCollection()
    {
        Func<IQueryable<TestPerson>, IQueryable<TestPerson>> queryShaper =
            q => q.Where(a => a.FirstName.Equals("TestFirstName") && a.LastName.Equals("TestLastName"));
        var result = await _peopleRepository.QueryCollectionAsync(queryShaper);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProjectionAsync_EntityExists_ReturnsEntityProjection()
    {
        var firstName = "TestFirstName";
        var lastName = "TestLastName";
        await DbService.PopulateDatabaseAsync(_context, 5);
        var person = DbService.GetPerson();
        person.FirstName = firstName;
        person.LastName = lastName;
        await _context.AddAsync(person);
        await _context.SaveChangesAsync();

        var resultProjection = await _peopleRepository.GetProjectionAsync(
            p => new { p.LastName }, p => p.FirstName == firstName);

        Assert.NotNull(resultProjection);
        Assert.Equal(lastName, resultProjection!.LastName);
    }

    [Fact]
    public async Task GetProjectionAsync_EntityDoesNotExist_ReturnsNull()
    {
        var resultProjection = await _peopleRepository.GetProjectionAsync(
            p => new { p.LastName }, p => p.FirstName == "TestFirstName");

        Assert.Null(resultProjection);
    }

    [Fact]
    public async Task GetProjectionsAsync_EntitiesExist_ReturnsEntitiesProjections()
    {
        var firstName = "TestFirstName";
        var lastName = "TestLastName";
        await DbService.PopulateDatabaseAsync(_context, 5);
        var person1 = DbService.GetPerson();
        person1.FirstName = firstName;
        person1.LastName = lastName;
        var person2 = DbService.GetPerson();
        person2.FirstName = firstName;
        person2.LastName = lastName;
        await _context.AddRangeAsync(person1, person2);
        await _context.SaveChangesAsync();

        var resultProjections = (await _peopleRepository.GetProjectionsAsync(
            p => new { p.LastName }, p => p.FirstName == firstName)).ToList();

        Assert.NotNull(resultProjections);
        Assert.Equal(2, resultProjections!.Count);
        Assert.Equal(lastName, resultProjections![0].LastName);
        Assert.Equal(lastName, resultProjections![1].LastName);
    }

    [Fact]
    public async Task GetProjectionsAsync_EntitiesDoNotExist_ReturnsEmptyCollection()
    {
        var resultProjections = await _peopleRepository.GetProjectionsAsync(
                       p => new { p.LastName }, p => p.FirstName == "TestFirstName");
        Assert.NotNull(resultProjections);
        Assert.Empty(resultProjections);
    }
}