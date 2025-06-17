using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.DataPersistence;

public interface IBaseRepository
{
    Guid InstanceId { get; }
    ILogger? Logger { get; }
    bool SaveChanges { get; set; }
    IContext Context { get; set; }
}

public interface IBaseRepository<TEntity, TId> : IBaseRepository  where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync();

    Task<IEnumerable<TEntity>> GetAllPagedAsync(int pageNumber, int pageSize);

    Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate);

    Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize);

    Task<TEntity?> GetByIdAsync(TId id);

    Task<TEntity> AddAsync(TEntity entity, string addedBy, CancellationToken cancellationToken);

    Task<TEntity> UpdateAsync(TEntity entity, string updatedBy, CancellationToken cancellationToken);

    Task DeleteAsync(TId id, string deletedBy, CancellationToken cancellationToken);

    IEnumerable<TEntity> GetAll();

    IEnumerable<TEntity> GetAllPaged(int pageNumber, int pageSize);

    IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate);

    IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize);

    TEntity? GetById(TId id);

    TEntity Add(TEntity entity, string addedBy);

    TEntity Update(TEntity entity, string updatedBy);

    void Delete(TId id, string deletedBy);
}
