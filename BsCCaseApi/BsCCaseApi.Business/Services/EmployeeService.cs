using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;

namespace BsCCaseApi.Business.Services;

public class EmployeeService(AppDbContext dbContext, ISyncEventService syncEventService) : IEmployeeService
{
    public async Task<Employee> GetEmployeeById(Guid employeeId)
    {
        var employee = await dbContext.Employees.FindAsync(employeeId);
        return employee ?? throw new Exception("Employee not found");
    }

    public async Task CreateEmployee(Employee employee)
    {
        var created = await dbContext.Employees.AddAsync(employee);
        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = created.Entity.Id,
            TableName = "Employees",
            Type = SyncType.Create
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<Employee> UpdateEmployee(Employee employee)
    {
        var updated = dbContext.Employees.Update(employee);
        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = updated.Entity.Id,
            TableName = "Employees",
            Type = SyncType.Update
        });
        await dbContext.SaveChangesAsync();
        return updated.Entity;
    }
}