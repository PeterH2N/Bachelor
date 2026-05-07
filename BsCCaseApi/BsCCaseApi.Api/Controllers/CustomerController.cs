using BsCCaseApi.Commons.Models;
using BsCCaseApi.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class CustomerController(ICustomerService customerService)
{
    [HttpGet("{customerId:int}")]
    public Task<Customer> Get(int customerId)
    {
        return customerService.GetCustomerById(customerId);
    }

    [HttpPut]
    public Task Create([FromBody] Customer customer)
    {
        return customerService.CreateCustomer(customer);
    }

    [HttpPatch]
    public Task<Customer> Update([FromBody] Customer customer)
    {
        return customerService.UpdateCustomer(customer);
    }
}