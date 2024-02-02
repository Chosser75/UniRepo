using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using UniRepo.Interfaces;

namespace UniRepo.Repositories;

public partial class UniversalRepository<TDbContext, TEntity, TIdType> : IUniversalRepository<TDbContext, TEntity, TIdType>
    where TDbContext : DbContext
    where TEntity : class, IUniRepoEntity<TIdType>
{
    private readonly TDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public UniversalRepository(TDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    /// <inheritdoc />
    public IEnumerable<TEntity> GetAll(bool isReadonly = false) =>
        isReadonly
        ? _dbSet.AsNoTracking()
        : _dbSet;

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(TIdType id, bool isReadonly = false)
    {
        ArgumentNullException.ThrowIfNull(id);

        return isReadonly
            ? await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id != null && e.Id.Equals(id))
            : await _dbSet.FindAsync(id);
    }


    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(IEnumerable<TIdType> keys, bool isReadonly = false)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var keysArray = keys.ToArray();

        if (_context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey() is not { } compositeKey)
            throw new InvalidOperationException($"Primary key for entity of type {typeof(TEntity).Name} not found.");

        var keyProperties = compositeKey.Properties;

        if (keysArray.Length != keyProperties.Count)
            throw new ArgumentException($"The number of provided keys does not match the number of keys for entity of type {typeof(TEntity).Name}.");

        var (parameter, predicate) = GetQueryExpression(keysArray, keyProperties);

        var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);

        return isReadonly
            ? await _dbSet.AsNoTracking().Where(lambda).FirstOrDefaultAsync()
            : await _dbSet.Where(lambda).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<TIdType> CreateAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateModifiedPropertiesAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var existingEntity = await _dbSet.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.Id != null && x.Id.Equals(entity.Id))
            ?? throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with id {entity.Id} not found.");

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TIdType id)
    {
        var entity = await GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with id {id} not found and therefore cannot be deleted.");

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<TResult?> QuerySingleAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryShaper)
    {
        var shapedQuery = queryShaper(_dbSet);

        return await shapedQuery.FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TResult>> QueryCollectionAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryShaper)
    {
        var shapedQuery = queryShaper(_dbSet);

        return await shapedQuery.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<TProjection?> GetProjectionAsync<TProjection>(Expression<Func<TEntity, TProjection>> projection, Guid entityId)
    {
        return await _dbSet
            .IgnoreAutoIncludes()
            .AsNoTracking()
            .Where(o => o.Id != null && o.Id.Equals(entityId))
            .Select(projection)
            .FirstOrDefaultAsync();
    }

    #region --------------------------- Private methods ---------------------------

    private static (ParameterExpression, Expression) GetQueryExpression(TIdType[] keys, IReadOnlyList<IProperty> keyProperties)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression? predicate = null;

        for (var i = 0; i < keyProperties.Count; i++)
        {
            var keyProperty = keyProperties[i];
            var keyName = keyProperty.Name;
            var keyVal = Expression.Constant(keys[i], typeof(TIdType));

            var property = Expression.Property(parameter, keyName);
            var equals = Expression.Equal(property, keyVal);

            predicate = predicate == null ? equals : Expression.AndAlso(predicate, equals)
                ?? throw new InvalidOperationException("No key properties found.");
        }

        return (parameter, predicate!);
    }

    #endregion #region --------------------------- Private methods ---------------------------
}