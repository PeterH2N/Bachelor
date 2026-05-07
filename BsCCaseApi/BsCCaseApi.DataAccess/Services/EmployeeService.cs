using BsCCaseApi.Commons.Models;
using BsCCaseApi.Library.Store;

namespace BsCCaseApi.Library.Services;

public class EmployeeService(AppDbContext dbContext) : IEmployeeService
{
    public async Task<Employee> GetEmployeeById(int employeeId)
    {
        var employee = await dbContext.Employees.FindAsync(employeeId);
        return employee ?? throw new Exception("Employee not found");
    }

    public async Task CreateEmployee(Employee employee)
    {
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Employee> UpdateEmployee(Employee employee)
    {
        var updated = dbContext.Employees.Update(employee);
        await dbContext.SaveChangesAsync();
        return updated.Entity;
    }
}