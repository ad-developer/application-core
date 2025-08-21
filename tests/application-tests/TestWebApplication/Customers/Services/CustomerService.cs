using ApplicationCore.Logging;
using ApplicationCore.Rules;
using ApplicationCore.Services;
using TestWebApplication.Customers.Models;
using TestWebApplication.Customers.Repositories;

namespace TestWebApplication.Customers.Services;

public class CustomerService : BaseService<CustomerService, ICustomerRepository>, ICustomerService
{
    public CustomerService(IRulePipeline rulePipeline, ITrackingLogger<CustomerService> trackingLogger, ICustomerRepository repositoryOne) 
    : base(rulePipeline, trackingLogger, repositoryOne)
    {
    }

    public Task AddCustomerAsync(Customer customer)
    {
        throw new NotImplementedException();
    }

    public Task<Customer> GetCustomerByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
