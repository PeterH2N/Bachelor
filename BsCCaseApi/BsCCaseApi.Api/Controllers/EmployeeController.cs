using BsCCaseApi.Commons.Models;
using BsCCaseApi.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class EmployeeController(IEmployeeService employeeService)
{
    [HttpGet("{employeeId:int}")]
    public Task<Employee> Get(int employeeId)
    {
        return employeeService.GetEmployeeById(employeeId);
    }

    [HttpPut]
    public Task Create([FromBody] Employee employee)
    {
        return employeeService.CreateEmployee(employee);
    }

    [HttpPatch]
    public Task<Employee> Update([FromBody] Employee employee)
    {
        return employeeService.UpdateEmployee(employee);
    }
}