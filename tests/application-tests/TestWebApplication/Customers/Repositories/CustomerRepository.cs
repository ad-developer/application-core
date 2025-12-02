using ApplicationCore.Caching;
using ApplicationCore.Data;
using ApplicationCore.DataPersistence;
using TestWebApplication.Customers.Entites;

namespace TestWebApplication.Customers.Repositories;

public class CustomerRepository : BaseRepository<CustomerRepository, Customer, Guid>, ICustomerRepository
{
    public CustomerRepository(IContext context, ILogger<CustomerRepository> trackingLogger, ICacheService cacheService) 
    : base(context, trackingLogger, cacheService)
    {
    }
}
