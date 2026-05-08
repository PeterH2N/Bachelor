using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.DataAccess.Services;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;

namespace BsCCaseApi.Business.Services;

public class CustomerService(AppDbContext dbContext, ISyncEventService syncEventService) : ICustomerService
{
    public async Task<Customer> GetCustomerById(Guid customerId)
    {
        var customer = await dbContext.Customers.FindAsync(customerId);
        return customer ?? throw new Exception("Customer not found");
    }

    public async Task CreateCustomer(Customer customer)
    {
        var created = await dbContext.Customers.AddAsync(customer);
        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = created.Entity.Id,
            TableName = "Customers",
            Type = SyncType.Create
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<Customer> UpdateCustomer(Customer customer)
    {
        var updated = dbContext.Customers.Update(customer);
        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = updated.Entity.Id,
            TableName = "Customers",
            Type = SyncType.Update
        });
        await dbContext.SaveChangesAsync();
        return updated.Entity;
    }
}