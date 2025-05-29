using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microblogging.Repository;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T?>> GetAllAsync();
    Task<IEnumerable<T?>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties);
    Task<IEnumerable<T?>> FindAsync(Expression<Func<T?, bool>> predicate);
    Task<IEnumerable<T?>> FindIncludingAsync(Expression<Func<T?, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
    Task AddAsync(T? entity);
    Task Update(T? entity);
    Task Remove(T? entity);
    Task RemoveRange(List<T> entity);
    Task AddRangeAsync(List<T> entities);

    IQueryable<T> Search(Expression<Func<T, bool>> Condition, string[]? Includes = null, int? PageSize = null, int? PageNumber = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        bool isTrack = true);

    Task<bool> SaveDbAsync();

}