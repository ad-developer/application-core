using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.DataPersistence;

public abstract class BaseRepository<TEntity, TId>(IContext context) : IBaseRepository<TEntity, TId>, IDisposable 
                                        where TEntity : class, IEntity<TId> 
{
    public IContext Context { get; set;} = context;

    public DbSet<TEntity> Entity  => context.Set<TEntity>();

    public bool SaveChanges { get; set; } = true;
    public Guid InstanceId  => Guid.NewGuid();
    public ILogger? Logger { get;}

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await Entity.ToListAsync();
    }
    
    public virtual async Task<IEnumerable<TEntity>> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        return await Entity.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }
    
    public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Entity.Where(predicate).ToListAsync();
    }
    
    public virtual async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize)
    {
        return await Entity.Where(predicate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }
    
    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {    
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return await Entity.FindAsync(id);
    }
    
    public virtual async Task<TEntity> AddAsync(TEntity entity, string addedBy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(addedBy, nameof(addedBy));

        entity.AddedBy = addedBy;
        entity.AddedDateTime = DateTime.UtcNow;
        entity.IsDeleted = false;

        await Entity.AddAsync(entity);
        if(SaveChanges)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        Logger?.LogInformation($"Entity of type {typeof(TEntity).Name} added by {addedBy} at {entity.AddedDateTime}");

        return entity;
    }
    
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, string updatedBy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(updatedBy, nameof(updatedBy));
        
        entity.UpdatedBy = updatedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;

        Context.Entry(entity).State = EntityState.Modified;
        if(SaveChanges)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        Logger?.LogInformation($"Entity of type {typeof(TEntity).Name} updated by {updatedBy} at {entity.UpdatedDateTime}");
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
        Logger?.LogInformation($"Entity of type {typeof(TEntity).Name} deleted by {deletedBy} at {entity.UpdatedDateTime}");
    }

    public virtual IEnumerable<TEntity> GetAll()
    {
        return Entity.ToList();
    }

    public virtual IEnumerable<TEntity> GetAllPaged(int pageNumber, int pageSize)
    {
        return Entity.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
    }
    
    public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
    {
        return Entity.Where(predicate).ToList();
    }

    public virtual IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize)
    {
        return Entity.Where(predicate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
    }
    
    public virtual TEntity? GetById(TId id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return Entity.Find(id);
    }

    public virtual TEntity Add(TEntity entity, string addedBy)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(addedBy, nameof(addedBy));

        entity.AddedBy = addedBy;
        entity.AddedDateTime = DateTime.UtcNow;
        entity.IsDeleted = false;

        Entity.Add(entity);
        if(SaveChanges)
        {
            Context.SaveChanges();
        }
        Logger?.LogInformation($"Entity of type {typeof(TEntity).Name} added by {addedBy} at {entity.AddedDateTime}");
        return entity;
    }

    public virtual TEntity Update(TEntity entity, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(updatedBy, nameof(updatedBy));

        entity.UpdatedBy = updatedBy;
        entity.UpdatedDateTime = DateTime.UtcNow;
    
        Context.Entry(entity).State = EntityState.Modified;
        if(SaveChanges)
        {
            Context.SaveChanges();
        }
        Logger?.LogInformation($"Entity of type {typeof(TEntity).Name} updated by {updatedBy} at {entity.UpdatedDateTime}");
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
        Logger?.LogInformation($"Entity of type {typeof(TEntity).Name} deleted by {deletedBy} at {entity.UpdatedDateTime}");
    }
    
    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        Logger?.LogInformation($"Disposing {GetType().Name} with InstanceId: {InstanceId}");

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
}
