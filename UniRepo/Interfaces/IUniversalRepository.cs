using System.Linq.Expressions;

namespace UniRepo.Interfaces;

/// <summary>
/// Represents a universal repository offering a comprehensive range of operations, including CRUD and advanced functionality, for entities in a database context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context. Must be a subclass of <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TEntity">The type of the entity this repository is responsible for. Must be a class that implements <see cref="IUniRepoEntity{TIdType}"/>.</typeparam>
/// <typeparam name="TIdType">The type of the identifier used by entities in this repository.</typeparam>
/// <remarks>
/// This class serves as a generic repository that abstracts away the common database operations like adding, removing, or querying entities.
/// It is designed to work with any entity type that implements the <see cref="IUniRepoEntity{TIdType}"/> interface, allowing for flexibility in defining different types of entities.
/// The repository is tightly coupled with a specific <see cref="DbContext"/> derived type, specified by <typeparamref name="TDbContext"/>, facilitating operations within that specific database context.
/// </remarks>
public interface IUniversalRepository<TDbContext, TEntity, TIdType>
{
    /// <summary>
    /// Retrieves all entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="isReadonly">
    /// A boolean value indicating whether the entities should be retrieved in a read-only mode (AsNoTracking). 
    /// If set to true, entities are retrieved without tracking changes (more efficient for read-only scenarios). 
    /// If false, entities are tracked by the DbContext, allowing for subsequent updates. Default is false.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{TEntity}"/> representing the collection of all entities.
    /// Entities are either tracked or non-tracked based on the <paramref name="isReadonly"/> parameter.
    /// </returns>
    /// <remarks>
    /// This method allows fetching the entire set of entities in a context. It's particularly useful for operations 
    /// where either a snapshot of the data is required without the need for tracking (read-only), or when the entities 
    /// might be updated and changes need to be tracked. Use the non-tracked option with care for large data sets as it 
    /// can significantly impact memory usage.
    /// </remarks>
    public IEnumerable<TEntity> GetAll(bool isReadonly = false);

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

    /// <summary>
    /// Asynchronously creates the specified entity in the database.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created entity's Id.</returns>
    Task<TIdType> CreateAsync(TEntity entity);

    /// <summary>
    /// Asynchronously updates the specified entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update. The entity can be in a detached state.</param>
    /// <returns>
    /// A task that represents the asynchronous update operation.
    /// </returns>
    /// <remarks>
    /// This method uses <c>_dbSet.Update(entity)</c> to update the entity in the database.
    /// When invoked, it performs the following operations:
    /// <list type="bullet">
    /// <item>
    /// <description>If the entity is not currently being tracked by the DbContext, it attaches the entity to the DbContext.</description>
    /// </item>
    /// <item>
    /// <description>It then marks the entire entity as modified. This means all property values of the entity will be sent to the database during the next call to <c>SaveChangesAsync</c>, regardless of whether they have actually changed.</description>
    /// </item>
    /// </list>
    /// This method is particularly effective for updating detached entities, such as those received from an API call, deserialized from JSON, or retrieved from a different DbContext instance.
    /// 
    /// However, it may not be the most efficient choice if only a few properties need to be updated, as it marks all properties as modified. Additionally, for large entities with many properties, this could lead to less efficient SQL updates.
    /// 
    /// If concurrent updates are a concern, or if you need to update specific properties selectively, consider using the <c>UpdateModifiedPropertiesAsync</c> method.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="entity"/> is null.</exception>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Asynchronously updates the specified entity in the database in the way that only modified properties are updated.
    /// </summary>
    /// <param name="entity">The entity to update. The entity can be in a detached state.</param>
    /// <returns>
    /// A task that represents the asynchronous update operation.
    /// </returns>
    /// <remarks>
    /// This method first retrieves the existing entity from the database and then applies the updated values.
    /// <list type="bullet">
    /// <item>
    /// <description>Selective Update: This method first retrieves the existing entity from the database and then applies the updated values. It's more selective as it only updates the properties that have actually changed.</description>
    /// </item>
    /// <item>
    /// <description>Error Checking: Includes a check to ensure the entity exists in the database before attempting to update it.</description>
    /// </item>
    /// /// <item>
    /// <description>Handling Detached Entities: Particularly useful when the entity being updated is detached from the DbContext (e.g., deserialized from a request). It ensures the existing entity in the context is updated.</description>
    /// </item>
    /// /// <item>
    /// <description>Performance Consideration: Involves an extra query to fetch the existing entity, which can be a performance overhead, especially for large datasets or complex entities.</description>
    /// </item>
    /// /// <item>
    /// <description>Concurrency Control: Provides a level of safety against concurrent updates, as it fetches the most recent state from the database before applying changes.</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="entity"/> is null.</exception>
    Task PatchAsync(TEntity entity);

    /// <summary>
    /// Asynchronously deletes the entity with specified Id from the database.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// /// <exception cref="InvalidOperationException">Thrown when no entry was found by the specified primary key.</exception>
    Task DeleteAsync(TIdType id);

    /// <summary>
    /// Asynchronously executes a query on the DbSet of TEntity that is expected to return a single result,
    /// allowing for custom shaping of the query through a function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result expected from the query.</typeparam>
    /// <param name="queryShaper">A function that takes an IQueryable of TEntity and returns an IQueryable of TResult.
    /// This function defines the query to be executed.</param>
    /// <returns>A Task representing the asynchronous operation. The Task.Result is the first instance of TResult
    /// found by the executed query, or null if no results are found.</returns>
    Task<TResult?> QuerySingleAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryShaper);

    /// <summary>
    /// Asynchronously executes a query on the DbSet of TEntity, allowing for custom shaping of the query through a function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result expected from the query.</typeparam>
    /// <param name="queryShaper">A function that takes an IQueryable of TEntity and returns an IQueryable of TResult.
    /// This function defines the query to be executed.</param>
    /// <returns>An IEnumerable of TResult, representing the result set of the executed query.</returns>
    Task<IEnumerable<TResult>> QueryCollectionAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryShaper);

    /// <summary>
    /// Asynchronously retrieves a projection of an entity of type <typeparamref name="TEntity"/> based on a specified projection expression.
    /// </summary>
    /// <typeparam name="TProjection">The type of the projection that is to be returned.</typeparam>
    /// <param name="projection">An expression that specifies how to project the entity into <typeparamref name="TProjection"/>.</param>
    /// <param name="entityId">The unique identifier (GUID) of the entity to be retrieved.</param>
    /// <remarks>
    /// This method queries the database asynchronously for an entity of type <typeparamref name="TEntity"/> that matches the provided entity ID. 
    /// It then applies the given projection expression to transform the entity into a <typeparamref name="TProjection"/> type.
    /// The query ignores any configured automatic includes and does not track the retrieved entity, optimizing performance for read-only scenarios.
    /// </remarks>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the projected entity of type <typeparamref name="TProjection"/>. 
    /// Returns null if the entity with the specified ID is not found.
    /// </returns>
    Task<TProjection?> GetProjectionAsync<TProjection>(Expression<Func<TEntity, TProjection>> projection, Guid entityId);

    /// <summary>
    /// Retrieves a collection of projections of an entity of type <typeparamref name="TEntity"/> based on a specified projection expression and filter expression.
    /// </summary>
    /// <typeparam name="TProjection">The type of the projection that is to be returned.</typeparam>
    /// <param name="projection">An expression that specifies how to project the entity into <typeparamref name="TProjection"/>.</param>
    /// <param name="filter">An expression that specifies how to filter entities in the <c>Where</c> method.</param>
    /// <remarks>
    /// This method queries the database for entities of type <typeparamref name="TEntity"/> that match the provided filter criteria. 
    /// It then applies the given projection expression to transform the entities into a <typeparamref name="TProjection"/> type.
    /// The query ignores any configured automatic includes and does not track retrieved entities, optimizing performance for read-only scenarios.
    /// </remarks>
    /// <returns>
    /// IEnumerable collection of projected entities of type <typeparamref name="TProjection"/>. 
    /// </returns>
    IEnumerable<TProjection> GetProjections<TProjection>(
        Expression<Func<TEntity, TProjection>> projection, Expression<Func<TEntity, bool>> filter);
}