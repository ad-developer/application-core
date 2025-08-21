using ApplicationCore.Caching;
using ApplicationCore.DataPersistence;
using ApplicationCore.Logging;
using TestWebApplication.Customers.Entites;

namespace TestWebApplication.Customers.Repositories;

public class CustomerRepository : BaseRepository<CustomerRepository, Customer, Guid>, ICustomerRepository
{
    public CustomerRepository(IContext context, ITrackingLogger<CustomerRepository> trackingLogger, ICacheService cacheService) 
    : base(context, trackingLogger, cacheService)
    {
    }
}
