using BsCCaseApi.Library.Models;

namespace BsCCaseApi.Library.Services;

public interface IEmployeeService
{
    public Task<Employee> GetEmployeeById(int employeeId);
    public Task CreateEmployee(Employee employee);
    public Task<Employee> UpdateEmployee(Employee employee);
}