using ApplicationCore.Services;
using TestWebApplication.Customers.Models;
using TestWebApplication.Customers.Repositories;

namespace TestWebApplication.Customers.Services;

public interface ICustomerService : IService<ICustomerRepository>
{
    Task<Customer> GetCustomerByIdAsync(Guid id);
    Task AddCustomerAsync(Customer customer);
}
