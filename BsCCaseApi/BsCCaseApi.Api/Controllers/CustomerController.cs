using BsCCaseApi.Business.Services;
using BsCCaseApi.Domain.Models;
using BsCCaseApi.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class CustomerController(ICustomerService customerService)
{
    [HttpGet("{customerId:guid}")]
    public Task<Customer> Get(Guid customerId)
    {
        return customerService.GetCustomerById(customerId);
    }

    [HttpPut]
    public Task Create([FromBody] CustomerDto customer)
    {
        return customerService.CreateCustomer(customer.ToCustomer());
    }

    [HttpPatch]
    public Task<Customer> Update([FromBody] CustomerDto customer)
    {
        return customerService.UpdateCustomer(customer.ToCustomer());
    }
}