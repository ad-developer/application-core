using ApplicationCore.Data;
using TestWebApplication.Customers.Entites;

namespace TestWebApplication.Customers.Repositories;

public interface ICustomerRepository : IBaseRepository<Customer, Guid>
{
}
