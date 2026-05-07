using BsCCaseApi.Commons.Models;

namespace BsCCaseApi.Library.Services;

public interface ICustomerService
{
    public Task<Customer> GetCustomerById(int customerId);
    public Task CreateCustomer(Customer customer);
    public Task<Customer> UpdateCustomer(Customer customer);
}