using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Business.Services;

public interface ICustomerService
{
    public Task<List<Customer>> GetAllCustomers();
    public Task<Customer> GetCustomerById(Guid customerId);
    public Task CreateCustomer(Customer customer);
    public Task<Customer> UpdateCustomer(Customer customer);
    public Task<List<Customer>> CreateRandomCustomer(int amount);
    public Task<Customer> UpdateRandomCustomer();
}