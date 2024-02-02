namespace UniRepo.Interfaces;

public interface IUniversalRepository<TEntity, in TIdType>
{
    /// <summary>
    /// Asynchronously retrieves an entity of type <typeparamref name="TEntity"/> based on a composite primary key.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve. The type of the identifier is <typeparamref name="TIdType"/>.</param>
    /// <param name="isReadonly">A boolean value indicating whether the entity should be retrieved in a read-only mode (AsNoTracking). 
    /// If set to true, the entity is retrieved without tracking changes (more efficient for read-only scenarios). 
    /// If false, the entity is tracked by the DbContext, allowing for subsequent updates.
    /// The parameter is optional. Default value is false.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the entity matching the provided identifier.
    /// If no entity is found, the task result is null.
    /// </returns>
    /// <remarks>
    /// This method offers the flexibility to fetch entities in either a tracked or non-tracked state. 
    /// Tracked entities are suitable for scenarios where updates to the entity are expected, as changes are tracked by the DbContext and can be persisted back to the database.
    /// Non-tracked entities are more efficient for read-only operations, as they do not incur the overhead of change tracking.
    /// </remarks>
    Task<TEntity?> GetByIdAsync(TIdType id, bool isReadonly = false);

    /// <summary>
    /// Asynchronously retrieves an entity of type <typeparamref name="TEntity"/> based on a composite primary key.
    /// Starts tracking found entity.
    /// </summary>
    /// <param name="keys">An IEnumerable collection of keys representing the composite primary key of the entity.
    /// The number and order of keys should match the entity's primary key definition.</param>
    /// <param name="isReadonly">A boolean value indicating whether the entity should be retrieved in a read-only mode (AsNoTracking). 
    /// If set to true, the entity is retrieved without tracking changes (more efficient for read-only scenarios). 
    /// If false, the entity is tracked by the DbContext, allowing for subsequent updates.
    /// The parameter is optional. Default value is false.</param>
    /// <remarks>
    /// Constructs a dynamic query based on the provided keys and the primary key properties of the <typeparamref name="TEntity"/>.
    /// Throws an <see cref="InvalidOperationException"/> if the primary key for the entity is not defined or if no key properties are found.
    /// An <see cref="ArgumentException"/> is thrown if the number of provided keys does not match the number of keys for the entity.
    /// </remarks>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the entity matching the provided keys.
    /// If no entity is found, or if the keys do not match, the task result is null.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the primary key definition for the entity is not found or if no key properties are found.</exception>
    /// <exception cref="ArgumentException">Thrown when the number of provided keys does not match the number of keys for the entity.</exception>
    Task<TEntity?> GetByIdAsync(IEnumerable<TIdType> keys, bool isReadonly = false);
}