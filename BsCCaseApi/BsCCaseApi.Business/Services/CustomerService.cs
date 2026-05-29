using BsCCaseApi.Business.Helpers;
using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BsCCaseApi.Business.Services;

public class CustomerService(AppDbContext dbContext, ISyncEventService syncEventService, IModelFaker modelFaker) : ICustomerService
{
    public async Task<List<Customer>> GetAllCustomers()
    {
        return await dbContext.Customers.ToListAsync();
    }

    public async Task<Customer> GetCustomerById(Guid customerId)
    {
        var customer = await dbContext.Customers.FindAsync(customerId);
        return customer ?? throw new Exception("Customer not found");
    }

    public async Task CreateCustomer(Customer customer)
    {
        await syncEventService.DoOperation<Customer>(SyncType.Create, customer);
    }

    public async Task<Customer> UpdateCustomer(Customer customer)
    {
        return await syncEventService.DoOperation<Customer>(SyncType.Update, customer);
    }

    public async Task<List<Customer>> CreateRandomCustomer(int amount)
    {
        var newCustomers = modelFaker.RandomCustomer(amount);
        foreach (var newCustomer in newCustomers)
        {
            await syncEventService.DoOperation<Customer>(SyncType.Create, newCustomer);
        }
        return newCustomers;
    }
    
    public async Task<Customer> UpdateRandomCustomer()
    {
        var updatedCustomer = modelFaker.RandomCustomer(1).First();
        // get random id
        var randomId = await dbContext.Set<Customer>()
            .OrderBy(e => EF.Functions.Random())
            .Select(e => e.Id)
            .FirstOrDefaultAsync();
        updatedCustomer.Id = randomId;
        
        await syncEventService.DoOperation<Customer>(SyncType.Update, updatedCustomer);
        return updatedCustomer;
    }
}