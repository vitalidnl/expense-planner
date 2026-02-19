using ExpensePlanner.Application;

namespace ExpensePlanner.DataAccess;

/// <summary>
/// Generic repository implementation placeholder for data persistence.
/// To be replaced with CSV or database implementations.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    /// <summary>
    /// Get all entities asynchronously.
    /// </summary>
    public Task<IEnumerable<T>> GetAllAsync() =>
        Task.FromResult(Enumerable.Empty<T>());

    /// <summary>
    /// Get a single entity by ID asynchronously.
    /// </summary>
    public Task<T?> GetByIdAsync(int id) =>
        Task.FromResult((T?)null);

    /// <summary>
    /// Add a new entity asynchronously.
    /// </summary>
    public Task AddAsync(T entity) =>
        Task.CompletedTask;

    /// <summary>
    /// Update an existing entity asynchronously.
    /// </summary>
    public Task UpdateAsync(T entity) =>
        Task.CompletedTask;

    /// <summary>
    /// Delete an entity by ID asynchronously.
    /// </summary>
    public Task DeleteAsync(int id) =>
        Task.CompletedTask;
}
