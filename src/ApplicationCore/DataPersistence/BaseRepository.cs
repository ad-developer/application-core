using System.Linq.Expressions;
using ApplicationCore.Caching;
using ApplicationCore.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.DataPersistence;

public abstract class BaseRepository<TRepository, TEntity, TId> : IBaseRepository<TEntity, TId>, IDisposable
                                        where TEntity : class, IEntity<TId>
{
    private readonly ICacheService _cacheService;
    public IContext Context { get; set; }

    public DbSet<TEntity> Entity => Context.Set<TEntity>();

    public bool SaveChanges { get; set; } = true;

    public Guid InstanceId => Guid.NewGuid();

    public ITrackingLogger TrackingLogger { get; set; }

    public BaseRepository(IContext context,  ITrackingLogger<TRepository> trackingLogger, ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(trackingLogger, nameof(trackingLogger));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));

        Context = context;
        TrackingLogger = trackingLogger;
        _cacheService = cacheService;

        TrackingLogger.LogInformation($"{GetType().Name} initialized.");
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CacheToken? cacheToken = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey("GetAllAsync");

        if (cacheToken?.CacheData == true)
        {
            var cachedData = await _cacheService.GetObjectAsync<IEnumerable<TEntity>>(cacheKey, cacheToken.CacheType).ConfigureAwait(false);

            if (cachedData is not null)
                return cachedData;
        }

        var entities = await Entity.ToListAsync(cancellationToken);
        
        if (cacheToken?.CacheData == true)
        {
            await _cacheService.AddObjectAsync(cacheKey, entities, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entities;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllPagedAsync(int pageNumber, int pageSize, CacheToken? cacheToken = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey("GetAllPagedAsync", $"{pageNumber}_{pageSize}");
        if (cacheToken?.CacheData == true)
        {
            var cachedData = await _cacheService.GetObjectAsync<IEnumerable<TEntity>>(cacheKey, cacheToken.CacheType).ConfigureAwait(false);

            if (cachedData is not null)
                return cachedData;
        }

        var entities = await Entity.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        if (cacheToken?.CacheData == true)
        {
            await _cacheService.AddObjectAsync(cacheKey, entities, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entities;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CacheToken? cacheToken = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var cacheKey = GenerateCacheKey("GetAsync", predicate.ToString());

        if (cacheToken?.CacheData == true)
        {
            var cachedData = await _cacheService.GetObjectAsync<IEnumerable<TEntity>>(cacheKey, cacheToken.CacheType).ConfigureAwait(false);

            if (cachedData is not null)
                return cachedData;
        }

        var entities = await Entity.Where(predicate).ToListAsync(cancellationToken);

        if (cacheToken?.CacheData == true)
        {
            await _cacheService.AddObjectAsync(cacheKey, entities, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entities;
    }

    public virtual async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CacheToken? cacheToken = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var cacheKey = GenerateCacheKey("GetPagedAsync", $"{predicate.ToString()}_{pageNumber}_{pageSize}");
        if (cacheToken?.CacheData == true)
        {
            var cachedData = await _cacheService.GetObjectAsync<IEnumerable<TEntity>>(cacheKey, cacheToken.CacheType).ConfigureAwait(false);

            if (cachedData is not null)
                return cachedData;
        }

        var entities = await Entity.Where(predicate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        if (cacheToken?.CacheData == true)
        {
            await _cacheService.AddObjectAsync(cacheKey, entities, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entities;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CacheToken? cacheToken = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));

        var cacheKey = GenerateCacheKey("GetByIdAsync", id.ToString());

        if (cacheToken?.CacheData == true)
        {
            var cachedEntity = await _cacheService.GetObjectAsync<TEntity>(cacheKey, cacheToken.CacheType).ConfigureAwait(false);

            if (cachedEntity is not null)
                return cachedEntity;
        }

        var entity = await Entity.FindAsync(id, cancellationToken);

        if (entity is not null && cacheToken?.CacheData == true)
        {
            await _cacheService.AddObjectAsync(cacheKey, entity, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entity;
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, string addedBy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(addedBy, nameof(addedBy));

        entity.AddedBy = addedBy;
        entity.AddedDateTime = DateTime.UtcNow;
        entity.IsDeleted = false;

        await Entity.AddAsync(entity);
        if (SaveChanges)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }

        TrackingLogger.LogInformation($"Entity of type {typeof(TEntity).Name} added async.", InstanceId);
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, string updatedBy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(updatedBy, nameof(updatedBy));

        entity.UpdatedBy = updatedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;

        Context.Entry(entity).State = EntityState.Modified;
        if (SaveChanges)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }

        TrackingLogger.LogInformation($"Entity of type {typeof(TEntity).Name} updated async.", InstanceId);
        return entity;
    }

    public virtual async Task DeleteAsync(TId id, string deletedBy, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(deletedBy, nameof(deletedBy));
        ArgumentNullException.ThrowIfNull(id, nameof(id));

        var entity = await GetByIdAsync(id);
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity), $"Entity {nameof(entity)}not found");
        }

        entity.UpdatedBy = deletedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;
        entity.IsDeleted = true;

        if (SaveChanges)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }

        TrackingLogger.LogInformation($"Entity of type {typeof(TEntity).Name} deleted async.", InstanceId);
    }

    public virtual IEnumerable<TEntity> GetAll(CacheToken? cacheToken = null)
    {
        var cacheKey = GenerateCacheKey("GetAll");

        if (cacheToken?.CacheData == true)
        {

            var cachedData = _cacheService.GetObject<IEnumerable<TEntity>>(cacheKey, cacheToken.CacheType);
            if (cachedData is not null)
                return cachedData;
        }

        var entities = Entity.ToList();

        if (cacheToken?.CacheData == true)
        {
            _cacheService.AddObject(cacheKey, entities, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entities;
    }

    public virtual IEnumerable<TEntity> GetAllPaged(int pageNumber, int pageSize, CacheToken? cacheToken = null)
    {
        var cacheKey = GenerateCacheKey("GetAllPaged", $"{pageNumber}_{pageSize}");

        if (cacheToken?.CacheData == true)
        {
            var cachedData = _cacheService.GetObject<IEnumerable<TEntity>>(cacheKey, cacheToken.CacheType);
            if (cachedData is not null)
                return cachedData;
        }

        var entities = Entity.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        if (cacheToken?.CacheData == true)
        {
            _cacheService.AddObject(cacheKey, entities, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entities;
    }

    public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, CacheToken? cacheToken = null)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        var cacheKey = GenerateCacheKey("Get", predicate.ToString());

        if (cacheToken?.CacheData == true)
        {
            var cachedData = _cacheService.GetObject<IEnumerable<TEntity>>(cacheKey, cacheToken.CacheType);
            if (cachedData is not null)
                return cachedData;
        }

        var entities = Entity.Where(predicate).ToList();
        if (cacheToken?.CacheData == true)
        {
            _cacheService.AddObject(cacheKey, entities, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entities;
    }

    public virtual IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CacheToken? cacheToken = null)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        var cacheKey = GenerateCacheKey("GetPaged", $"{predicate.ToString()}_{pageNumber}_{pageSize}");

        if (cacheToken?.CacheData == true)
        {
            var cachedData = _cacheService.GetObject<IEnumerable<TEntity>>(cacheKey, cacheToken.CacheType);
            if (cachedData is not null)
                return cachedData;
        }

        var entities = Entity.Where(predicate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        if (cacheToken?.CacheData == true)
        {
            _cacheService.AddObject(cacheKey, entities, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entities;
    }

    public virtual TEntity? GetById(TId id, CacheToken? cacheToken = null)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        var cacheKey = GenerateCacheKey("GetById", id.ToString());

        if (cacheToken?.CacheData == true)
        {
            var cachedEntity = _cacheService.GetObject<TEntity>(cacheKey, cacheToken.CacheType);
            if (cachedEntity is not null)
                return cachedEntity;
        }

        var entity = Entity.Find(id);

        if (entity is not null && cacheToken?.CacheData == true)
        {
            _cacheService.AddObject(cacheKey, entity, cacheToken.CacheType, cacheToken.ExpirationType, cacheToken.ExpirationTime);
        }

        return entity;
    }

    public virtual TEntity Add(TEntity entity, string addedBy)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(addedBy, nameof(addedBy));

        entity.AddedBy = addedBy;
        entity.AddedDateTime = DateTime.UtcNow;
        entity.IsDeleted = false;

        Entity.Add(entity);
        if (SaveChanges)
        {
            Context.SaveChanges();
        }

        TrackingLogger.LogInformation($"Entity of type {typeof(TEntity).Name} added.", InstanceId);
        return entity;
    }

    public virtual TEntity Update(TEntity entity, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(updatedBy, nameof(updatedBy));

        entity.UpdatedBy = updatedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;

        Context.Entry(entity).State = EntityState.Modified;
        if (SaveChanges)
        {
            Context.SaveChanges();
        }

        TrackingLogger.LogInformation($"Entity of type {typeof(TEntity).Name} updated.", InstanceId);
        return entity;
    }

    public virtual void Delete(TId id, string deletedBy)
    {
        ArgumentException.ThrowIfNullOrEmpty(deletedBy, nameof(deletedBy));
        ArgumentNullException.ThrowIfNull(id, nameof(id));

        var entity = GetById(id);
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity), $"Entity {nameof(entity)}not found");
        }

        entity.UpdatedBy = deletedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;
        entity.IsDeleted = true;

        if (SaveChanges)
        {
            Context.SaveChanges();
        }

        TrackingLogger.LogInformation($"Entity of type {typeof(TEntity).Name} deleted", InstanceId);
    }

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        TrackingLogger.LogInformation($"Disposing {GetType().Name}", InstanceId);

        if (!_disposed)
        {
            if (disposing)
            {
                Context.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal string GenerateCacheKey(string action, string additionlKey = "")
    {
        return  $"{typeof(TEntity).Name}_{action}_{additionlKey}".ToLowerInvariant();
    }
}
