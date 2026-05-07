using BsCCaseApi.Commons.Models;
using BsCCaseApi.Library.Store;

namespace BsCCaseApi.Library.Services;

public class CustomerService(AppDbContext dbContext) : ICustomerService
{
    public async Task<Customer> GetCustomerById(int customerId)
    {
        var customer = await dbContext.Customers.FindAsync(customerId);
        return customer ?? throw new Exception("Customer not found");
    }

    public async Task CreateCustomer(Customer customer)
    {
        await dbContext.Customers.AddAsync(customer);
    }

    public async Task<Customer> UpdateCustomer(Customer customer)
    {
        var updated = dbContext.Customers.Update(customer);
        await dbContext.SaveChangesAsync();
        return updated.Entity;
    }
}