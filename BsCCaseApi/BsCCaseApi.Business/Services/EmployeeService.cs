using BsCCaseApi.Business.Helpers;
using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BsCCaseApi.Business.Services;

public class EmployeeService(AppDbContext dbContext, ISyncEventService syncEventService, IModelFaker modelFaker) : IEmployeeService
{
    public async Task<List<Employee>> GetAllEmployees()
    {
        return await dbContext.Employees.ToListAsync();
    }

    public async Task<Employee> GetEmployeeById(Guid employeeId)
    {
        var employee = await dbContext.Employees.FindAsync(employeeId);
        return employee ?? throw new Exception("Employee not found");
    }

    public async Task CreateEmployee(Employee employee)
    {
        await syncEventService.DoOperation<Employee>(SyncType.Create, employee);
    }

    public async Task<Employee?> UpdateEmployee(Employee employee)
    {
        return await syncEventService.DoOperation<Employee>(SyncType.Update, employee);
    }

    public async Task<List<Employee>> CreateRandomEmployee(int amount)
    {
        var newEmployees = modelFaker.RandomEmployee(amount);
        foreach (var newEmployee in newEmployees)
        {
            await syncEventService.DoOperation<Employee>(SyncType.Create, newEmployee);
        }
        return newEmployees;
    }
}