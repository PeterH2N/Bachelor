using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Business.Services;

public interface IEmployeeService
{
    public Task<Employee> GetEmployeeById(Guid employeeId);
    public Task CreateEmployee(Employee employee);
    public Task<Employee> UpdateEmployee(Employee employee);
}