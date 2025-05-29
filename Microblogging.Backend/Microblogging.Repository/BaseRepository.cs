using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Driver;

namespace Microblogging.Repository;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public BaseRepository(MongoDbContext dbContext)
    {
        _collection = dbContext.GetCollection<T>(typeof(T).Name + "s");
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        var prop = typeof(T).GetProperty("Id");
        var filter = Builders<T>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T?>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<IEnumerable<T?>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties)
        => await GetAllAsync(); // MongoDB doesn't support Include directly

    public async Task<IEnumerable<T?>> FindAsync(Expression<Func<T?, bool>> predicate) =>
        await _collection.Find(predicate!).ToListAsync();

    public async Task<IEnumerable<T?>> FindIncludingAsync(Expression<Func<T?, bool>> predicate, params Expression<Func<T, object>>[] includeProperties) =>
        await FindAsync(predicate); // same reason

    public async Task AddAsync(T? entity)
    {
        if (entity != null)
            await _collection.InsertOneAsync(entity);
    }

    public Task Update(T? entity)
    {
        var idProperty = typeof(T).GetProperty("Id")!;
        var id = idProperty.GetValue(entity)?.ToString();
        var filter = Builders<T>.Filter.Eq("Id", id);
        return _collection.ReplaceOneAsync(filter, entity!);
    }

    public Task Remove(T? entity)
    {
        var idProperty = typeof(T).GetProperty("Id")!;
        var id = idProperty.GetValue(entity)?.ToString();
        var filter = Builders<T>.Filter.Eq("Id", id);
        return _collection.DeleteOneAsync(filter);
    }

    public Task RemoveRange(List<T> entity)
    {
        var ids = entity.Select(e => typeof(T).GetProperty("Id")!.GetValue(e)!.ToString()).ToList();
        var filter = Builders<T>.Filter.In("Id", ids);
        return _collection.DeleteManyAsync(filter);
    }

    public Task AddRangeAsync(List<T> entities) =>
        _collection.InsertManyAsync(entities);

    public async Task<bool> SaveDbAsync() => await Task.FromResult(true); // not applicable for Mongo

    public Task<IClientSessionHandle> BeginTransactionAsync() =>
        new MongoClient().StartSessionAsync(); // Optional: only if you need transactions

    public IQueryable<T> Search(Expression<Func<T, bool>> Condition, string[]? Includes = null, int? PageSize = null, int? PageNumber = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, bool isTrack = true)
    {
        var result = _collection.AsQueryable().Where(Condition);
        if (orderBy != null)
            result = orderBy(result);
        return result;
    }

  
}
