using BsCCaseApi.Business.Services;
using BsCCaseApi.Domain.Models;
using BsCCaseApi.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class EmployeeController(IEmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    public Task<List<Employee>> GetAll()
    {
        return employeeService.GetAllEmployees();
    }
    
    [HttpGet("{employeeId:guid}")]
    public Task<Employee> Get(Guid employeeId)
    {
        return employeeService.GetEmployeeById(employeeId);
    }

    [HttpPut]
    public Task Create([FromBody] EmployeeDto employee)
    {
        return employeeService.CreateEmployee(employee.ToEmployee());
    }

    [HttpPatch("{employeeId:guid}")]
    public Task<Employee?> Update([FromBody] EmployeeDto employee, Guid employeeId)
    {
        var employeeToUpdate = employee.ToEmployee();
        employeeToUpdate.Id = employeeId;
        return employeeService.UpdateEmployee(employeeToUpdate);
    }
    
    [HttpPut]
    public Task<List<Employee>> CreateRandom([FromQuery]int amount = 1)
    {
        return employeeService.CreateRandomEmployee(amount);
    }
    
    [HttpPatch]
    public Task<Employee> UpdateRandom()
    {
        return employeeService.UpdateRandomEmployee();
    }
}