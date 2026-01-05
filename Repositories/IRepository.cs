using System.Linq.Expressions;

namespace UniStart.Repositories;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
public interface IRepository<T> where T : class
{
    // Query operations
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    
    // Query with includes
    IQueryable<T> Query();
    IQueryable<T> QueryWhere(Expression<Func<T, bool>> predicate);
    
    // Aggregation
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    
    // Modification operations
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
