using System.Linq.Expressions;
using ApplicationCore.Logging;

namespace ApplicationCore.DataPersistence;

public interface IBaseRepository 
{
    Guid InstanceId { get; }
    ITrackingLogger TrackingLogger { get; set; }
    bool SaveChanges { get; set; }
    IContext Context { get; set; }
}

public interface IBaseRepository<TEntity, TId> : IBaseRepository  where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync(CacheToken? cacheToken = null, CancellationToken cancellationTokenъ = default);

    Task<IEnumerable<TEntity>> GetAllPagedAsync(int pageNumber, int pageSize, CacheToken? cacheToken = null, CancellationToken cancellationTokenъ = default);

    Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CacheToken? cacheToken = null, CancellationToken cancellationTokenъ = default);

    Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CacheToken? cacheToken = null, CancellationToken cancellationTokenъ = default);

    Task<TEntity?> GetByIdAsync(TId id, CacheToken? cacheToken = null, CancellationToken cancellationTokenъ = default);

    Task<TEntity> AddAsync(TEntity entity, string addedBy,  CancellationToken cancellationTokenъ = default);

    Task<TEntity> UpdateAsync(TEntity entity, string updatedBy,  CancellationToken cancellationToken = default);

    Task DeleteAsync(TId id, string deletedBy,  CancellationToken cancellationToken = default);

    IEnumerable<TEntity> GetAll(CacheToken? cacheToken = null);

    IEnumerable<TEntity> GetAllPaged(int pageNumber, int pageSize, CacheToken? cacheToken = null);

    IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, CacheToken? cacheToken = null);

    IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CacheToken? cacheToken = null);

    TEntity? GetById(TId id, CacheToken? cacheToken = null);

    TEntity Add(TEntity entity, string addedBy);

    TEntity Update(TEntity entity, string updatedBy);

    void Delete(TId id, string deletedBy);
}
