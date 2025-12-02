using System.Linq.Expressions;
using System.Security.Principal;
using ApplicationCore.DataPersistence;


namespace ApplicationCore.Data;

public interface IBaseRepository 
{
    bool SaveChanges { get; set; }
    IContext Context { get; set; }
}

public interface IBaseRepository<TEntity, TId> : IBaseRepository, IDisposable
    where TEntity : IEntity<TId>
{
    #region Async Methods

    Task<IEnumerable<TEntity>> GetAllAsync(
        CacheToken? cacheToken = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        CacheToken? cacheToken = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CacheToken? cacheToken = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> predicate,
        int pageNumber,
        int pageSize,
        CacheToken? cacheToken = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(
        TId id,
        CacheToken? cacheToken = null,
        CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, string addedBy, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, string updatedBy, CancellationToken cancellationToken = default);
    Task DeleteAsync(TId id, string deletedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Sync Methods

    IEnumerable<TEntity> GetAll(CacheToken? cacheToken = null);

    PagedResult<TEntity> GetAllPaged(
        int pageNumber,
        int pageSize,
        CacheToken? cacheToken = null);

    IEnumerable<TEntity> Get(
        Expression<Func<TEntity, bool>> predicate,
        CacheToken? cacheToken = null);

    PagedResult<TEntity> GetPaged(
        Expression<Func<TEntity, bool>> predicate,
        int pageNumber,
        int pageSize,
        CacheToken? cacheToken = null);

    TEntity? GetById(TId id, CacheToken? cacheToken = null);

    TEntity Add(TEntity entity, string addedBy);
    TEntity Update(TEntity entity, string updatedBy);
    void Delete(TId id, string deletedBy);

    #endregion
}