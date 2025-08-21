using ApplicationCore.DataPersistence;
using TestWebApplication.Customers.Entites;

namespace TestWebApplication.Customers.Repositories;

public interface ICustomerRepository : IBaseRepository<Customer, Guid>
{
}
