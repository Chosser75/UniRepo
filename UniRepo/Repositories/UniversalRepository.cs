using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using UniRepo.Interfaces;

namespace UniRepo.Repositories;

public class UniversalRepository<TEntity, TIdType> : IUniversalRepository<TEntity, TIdType>
    where TEntity : class, IUniRepoEntity<TIdType>
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public UniversalRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

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