using System.Linq.Expressions;
using ApplicationCore.Caching;
using ApplicationCore.DataPersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Data;

public abstract class BaseRepository<TRepository, TEntity, TId> : IBaseRepository<TEntity, TId>, IDisposable
    where TEntity : class, IEntity<TId>
{

    public IContext Context { get; set; }
    public bool SaveChanges { get; set; } = true;
    private readonly ICacheService _cacheService;
    private bool _disposed;
    protected DbSet<TEntity> Entity => Context.Set<TEntity>();
    protected ILogger Logger { get; }
    protected Guid InstanceId { get; } = Guid.NewGuid();
    

    protected BaseRepository(IContext context, ILogger<TRepository> logger, ICacheService cacheService)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

        Logger.LogInformation($"{GetType().Name} initialized.");
    }

    #region Core Cache Helpers

    private async Task<T?> GetOrAddCacheAsync<T>(
        string key,
        Func<Task<T>> factory,
        CacheToken? cacheToken = null)
    {
        if (cacheToken?.CacheData != true)
            return await factory();

        return await _cacheService.GetOrAddObjectAsync(
            key,
            factory,
            cacheToken.UserIdentity,
            cacheToken.CacheType,
            cacheToken.ExpirationType,
            cacheToken.ExpirationTime);
    }

    private T? GetOrAddCache<T>(
        string key,
        Func<T> factory,
        CacheToken? cacheToken = null)
    {
        if (cacheToken?.CacheData != true)
            return factory();

        return _cacheService.GetOrAddObject(
            key,
            factory,
            cacheToken.UserIdentity,
            cacheToken.CacheType,
            cacheToken.ExpirationType,
            cacheToken.ExpirationTime);
    }

    private string GenerateCacheKey(string action, string? additionalKey = null)
        => $"{typeof(TEntity).Name}_{action}_{additionalKey}".ToLowerInvariant();

    #endregion

    #region Paging Helpers

    private async Task<PagedResult<TEntity>> BuildPagedResultAsync(
        IQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private PagedResult<TEntity> BuildPagedResult(
        IQueryable<TEntity> query,
        int pageNumber,
        int pageSize)
    {
        var totalCount = query.Count();
        var items = query.Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

        return new PagedResult<TEntity>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    #endregion

    #region Async Methods

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CacheToken? cacheToken = null, CancellationToken cancellationToken = default)
        => await GetOrAddCacheAsync(
        GenerateCacheKey(nameof(GetAllAsync)),
        async () => await Entity.ToListAsync(cancellationToken),
        cacheToken) ?? Enumerable.Empty<TEntity>();

    public virtual async Task<PagedResult<TEntity>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        CacheToken? cacheToken = null,
        CancellationToken cancellationToken = default)
    {
        return await GetOrAddCacheAsync(
            GenerateCacheKey(nameof(GetAllPagedAsync), $"{pageNumber}_{pageSize}"),
            () => BuildPagedResultAsync(Entity.AsQueryable(), pageNumber, pageSize, cancellationToken),
            cacheToken
        ) ?? new PagedResult<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CacheToken? cacheToken = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await GetOrAddCacheAsync(
            GenerateCacheKey(nameof(GetAsync), predicate.ToString()),
            () => Entity.Where(predicate).ToListAsync(cancellationToken),
            cacheToken) ?? Enumerable.Empty<TEntity>();
    }

  public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> predicate,
        int pageNumber,
        int pageSize,
        CacheToken? cacheToken = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await GetOrAddCacheAsync(
            GenerateCacheKey(nameof(GetPagedAsync), $"{predicate}_{pageNumber}_{pageSize}"),
            () => BuildPagedResultAsync(Entity.Where(predicate), pageNumber, pageSize, cancellationToken),
            cacheToken
        ) ?? new PagedResult<TEntity>();
    }


    public virtual Task<TEntity?> GetByIdAsync(TId id, CacheToken? cacheToken = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return GetOrAddCacheAsync(
            GenerateCacheKey(nameof(GetByIdAsync), id.ToString()),
            async () => await Entity.FindAsync(new object?[] { id }, cancellationToken),
            cacheToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, string addedBy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrEmpty(addedBy);

        entity.AddedBy = addedBy;
        entity.AddedDateTime = DateTime.UtcNow;
        entity.IsDeleted = false;

        await Entity.AddAsync(entity, cancellationToken);
        if (SaveChanges)
            await Context.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Entity of type {EntityType} added async. InstanceId: {InstanceId}", typeof(TEntity).Name, InstanceId);
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, string updatedBy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrEmpty(updatedBy);

        entity.UpdatedBy = updatedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;

        Context.Entry(entity).State = EntityState.Modified;
        if (SaveChanges)
            await Context.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Entity of type {EntityType} updated async. InstanceId: {InstanceId}", typeof(TEntity).Name, InstanceId);
        return entity;
    }

    public virtual async Task DeleteAsync(TId id, string deletedBy, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(deletedBy);
        ArgumentNullException.ThrowIfNull(id);

        var entity = await GetByIdAsync(id, null, cancellationToken);
        if (entity is null)
            throw new ArgumentNullException(nameof(entity), $"Entity with id {id} not found");

        entity.UpdatedBy = deletedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;
        entity.IsDeleted = true;

        if (SaveChanges)
            await Context.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Entity of type {EntityType} deleted async. InstanceId: {InstanceId}", typeof(TEntity).Name, InstanceId);
    }

    #endregion

    #region Sync Methods

    public virtual IEnumerable<TEntity> GetAll(CacheToken? cacheToken = null)
        => GetOrAddCache(
            GenerateCacheKey(nameof(GetAll)),
            () => Entity.ToList(),
            cacheToken)!;

    public virtual PagedResult<TEntity> GetAllPaged(
        int pageNumber,
        int pageSize,
        CacheToken? cacheToken = null)
    {
        return GetOrAddCache(
            GenerateCacheKey(nameof(GetAllPaged), $"{pageNumber}_{pageSize}"),
            () => BuildPagedResult(Entity.AsQueryable(), pageNumber, pageSize),
            cacheToken
        )!;
    }


    public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, CacheToken? cacheToken = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return GetOrAddCache(
            GenerateCacheKey(nameof(Get), predicate.ToString()),
            () => Entity.Where(predicate).ToList(),
            cacheToken)!;
    }

   public virtual PagedResult<TEntity> GetPaged(
        Expression<Func<TEntity, bool>> predicate,
        int pageNumber,
        int pageSize,
        CacheToken? cacheToken = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return GetOrAddCache(
            GenerateCacheKey(nameof(GetPaged), $"{predicate}_{pageNumber}_{pageSize}"),
            () => BuildPagedResult(Entity.Where(predicate), pageNumber, pageSize),
            cacheToken
        )!;
    }


    public virtual TEntity? GetById(TId id, CacheToken? cacheToken = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        return GetOrAddCache(
            GenerateCacheKey(nameof(GetById), id.ToString()),
            () => Entity.Find(id),
            cacheToken);
    }

    public virtual TEntity Add(TEntity entity, string addedBy)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrEmpty(addedBy);

        entity.AddedBy = addedBy;
        entity.AddedDateTime = DateTime.UtcNow;
        entity.IsDeleted = false;

        Entity.Add(entity);
        if (SaveChanges)
            Context.SaveChanges();

        Logger.LogInformation("Entity of type {EntityType} added. InstanceId: {InstanceId}", typeof(TEntity).Name, InstanceId);
        return entity;
    }

    public virtual TEntity Update(TEntity entity, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrEmpty(updatedBy);

        entity.UpdatedBy = updatedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;

        Context.Entry(entity).State = EntityState.Modified;
        if (SaveChanges)
            Context.SaveChanges();

        Logger.LogInformation("Entity of type {EntityType} updated. InstanceId: {InstanceId}", typeof(TEntity).Name, InstanceId);
        return entity;
    }

    public virtual void Delete(TId id, string deletedBy)
    {
        ArgumentException.ThrowIfNullOrEmpty(deletedBy);
        ArgumentNullException.ThrowIfNull(id);

        var entity = GetById(id);
        if (entity is null)
            throw new ArgumentNullException(nameof(entity), $"Entity with id {id} not found");

        entity.UpdatedBy = deletedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;
        entity.IsDeleted = true;

        if (SaveChanges)
            Context.SaveChanges();

        Logger.LogInformation("Entity of type {EntityType} deleted. InstanceId: {InstanceId}", typeof(TEntity).Name, InstanceId);
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
            Context.Dispose();

        Logger.LogInformation("Disposing {RepositoryName}. InstanceId: {InstanceId}", GetType().Name, InstanceId);
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
