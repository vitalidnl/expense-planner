namespace ExpensePlanner.Application;

/// <summary>
/// Generic repository interface for data persistence abstraction.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get all entities asynchronously.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Get a single entity by ID asynchronously.
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Add a new entity asynchronously.
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Update an existing entity asynchronously.
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity by ID asynchronously.
    /// </summary>
    Task DeleteAsync(int id);
}
