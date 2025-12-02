namespace ApplicationCore.DataPersistence;


public interface IUnitOfWork
{
    void SaveChanges();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
