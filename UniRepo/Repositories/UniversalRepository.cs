using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using UniRepo.Interfaces;

namespace UniRepo.Repositories;

/// <summary>
/// Represents a universal repository offering a comprehensive range of operations, including CRUD and advanced functionality, for entities in a database context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context. Must be a subclass of <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TEntity">The type of the entity this repository is responsible for.</typeparam>
/// <remarks>
/// This class serves as a generic repository that abstracts away the common database operations like adding, removing, or querying entities.
/// It is designed to work with any Entity Framework Core DbContext and entity type.
/// The repository is tightly coupled with a specific <see cref="DbContext"/> derived type, specified by <typeparamref name="TDbContext"/>, facilitating operations within that specific database context.
/// </remarks>
public partial class UniversalRepository<TDbContext, TEntity> : IUniversalRepository<TDbContext, TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly TDbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private static readonly ConcurrentDictionary<Type,
        (PropertyInfo PropertyInfo, Func<object, object> IdConverter, IKey PrimaryKey)> Cache = new();

    public UniversalRepository(TDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual IEnumerable<TEntity> GetAll(bool isReadonly = false) =>
        isReadonly
        ? _dbSet.AsNoTracking()
        : _dbSet;

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(object id, bool isReadonly = false)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (id is IEnumerable<object> keys)
            return await GetByCompositeIdAsync(keys, isReadonly);

        var (keyProperty, idConverter, _) = GetPrimaryKeyProperties(typeof(TEntity), _context);

        var convertedId = idConverter(id);

        if (!isReadonly) return await _dbSet.FindAsync(convertedId);

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var property = Expression.Property(parameter, keyProperty.Name);
        var keyVal = Expression.Constant(convertedId, keyProperty.PropertyType);
        var equals = Expression.Equal(property, keyVal);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

        return await _dbSet.AsNoTracking().Where(lambda).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByCompositeIdAsync(IEnumerable<object> keys, bool isReadonly = false)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var (_, _, compositeKey) = GetPrimaryKeyProperties(typeof(TEntity), _context);
        if (compositeKey is null)
            throw new InvalidOperationException($"Primary key for entity of type {typeof(TEntity).Name} not found.");

        var keysArray = keys.ToArray();

        var keyProperties = compositeKey.Properties;

        if (keysArray.Length != keyProperties.Count)
            throw new ArgumentException($"The number of provided keys does not match the number of keys for entity of type {typeof(TEntity).Name}.");

        var (parameter, predicate) = GetQueryExpression(keysArray, keyProperties);

        var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);

        return isReadonly
            ? await _dbSet.AsNoTracking().Where(lambda).FirstOrDefaultAsync()
            : await _dbSet.Where(lambda).FirstOrDefaultAsync();
    }

    /// <summary>
    /// A simpler way to get an Entity by a composite key.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<TEntity?> GetByCompositeKeyAsync(object[] keys)
    {
        var compositeKey = _context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey();
        if (compositeKey == null || compositeKey.Properties.Count != keys.Length)
        {
            throw new InvalidOperationException($"Composite key mismatch or not found for entity {typeof(TEntity)}.");
        }

        var query = _dbSet.AsQueryable();

        for (var i = 0; i < keys.Length; i++)
        {
            var keyName = compositeKey.Properties[i].Name;
            var keyValue = keys[i];
            query = query.Where(e => EF.Property<object>(e, keyName).Equals(keyValue));
        }

        return await query.FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task PatchAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var (keyProperty, _, primaryKey) = GetPrimaryKeyProperties(typeof(TEntity), _context);

        var existingEntity = primaryKey.Properties.Count > 1
            ? await GetEntityByCompositeKeyAsync(entity, primaryKey)
            : await GetEntityBySingleKeyAsync(keyProperty, entity);

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(object id)
    {
        var entity = (id is IEnumerable<object> keys
            ? await GetByCompositeIdAsync(keys)
            : await GetByIdAsync(id))
                ?? throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with id {id} not found and therefore cannot be deleted.");

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task<TResult?> QuerySingleAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryShaper)
    {
        ArgumentNullException.ThrowIfNull(queryShaper);

        var shapedQuery = queryShaper(_dbSet);

        return await shapedQuery.FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TResult>> QueryCollectionAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> queryShaper)
    {
        ArgumentNullException.ThrowIfNull(queryShaper);

        var shapedQuery = queryShaper(_dbSet);

        return await shapedQuery.ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<TProjection?> GetProjectionAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection, Expression<Func<TEntity, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(filter);

        return await _dbSet
            .AsNoTracking()
            .Where(filter)
            .Select(projection)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection, Expression<Func<TEntity, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(filter);

        return await _dbSet
            .AsNoTracking()
            .Where(filter)
            .Select(projection)
            .ToListAsync();
    }

    #region --------------------------- Private methods ---------------------------

    private async Task<TEntity> GetEntityBySingleKeyAsync(PropertyInfo keyProperty, TEntity entity)
    {
        var entityId = keyProperty.GetValue(entity);

        if (entityId == null)
            throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} has a null value for its key property {keyProperty.Name}.");

        return await _dbSet.IgnoreAutoIncludes()
            .FirstOrDefaultAsync(e => EF.Property<object>(e, keyProperty.Name).Equals(entityId))
               ?? throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with id {entityId} not found.");
    }

    private async Task<TEntity> GetEntityByCompositeKeyAsync(TEntity entity, IKey primaryKey)
    {
        var keys = primaryKey.Properties.Select(p => GetPropertyInfoValue(p.PropertyInfo, entity)).ToArray();

        var existingEntity = await GetByCompositeIdAsync(keys);

        if (existingEntity is null)
            throw new InvalidOperationException(
                $"Entity of type {typeof(TEntity).Name} with id {string.Join(", ", keys.Select(k => k.ToString()))} not found.");

        return existingEntity!;
    }

    private object GetPropertyInfoValue(PropertyInfo? propertyInfo, TEntity entity) =>
        propertyInfo?.GetValue(entity)
        ?? throw new InvalidOperationException($"Primary key for entity type {entity.GetType().Name} not found.");

    private static (ParameterExpression, Expression) GetQueryExpression(object[] keys, IReadOnlyList<IProperty> keyProperties)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression? predicate = null;

        for (var i = 0; i < keyProperties.Count; i++)
        {
            var keyProperty = keyProperties[i];
            var keyName = keyProperty.Name;
            var keyType = keyProperty.ClrType;
            var keyVal = Expression.Constant(keys[i], keyType);

            var property = Expression.Property(parameter, keyName);
            var equals = Expression.Equal(property, keyVal);

            predicate = predicate == null ? equals : Expression.AndAlso(predicate, equals)
                                                     ?? throw new InvalidOperationException("No key properties found.");
        }

        return (parameter, predicate!);
    }

    private static (PropertyInfo PropertyInfo, Func<object, object> IdConverter, IKey PrimaryKey) GetPrimaryKeyProperties
        (Type entityType, DbContext context)
    {
        return Cache.GetOrAdd(entityType, type =>
        {
            var primaryKey = context.Model.FindEntityType(type)?.FindPrimaryKey();
            var propertyInfo = primaryKey?.Properties
                .FirstOrDefault()?.PropertyInfo;

            if (propertyInfo == null)
                throw new InvalidOperationException($"Primary key for entity type {type.Name} not found.");

            var parameter = Expression.Parameter(typeof(object), "id");

            var body = Expression.Convert(
                Expression.Convert(parameter, propertyInfo.PropertyType),
                typeof(object)
            );

            var convert = Expression.Lambda<Func<object, object>>(body, parameter).Compile();

            return (propertyInfo, convert, primaryKey!);
        });
    }

    #endregion #region --------------------------- Private methods ---------------------------
}