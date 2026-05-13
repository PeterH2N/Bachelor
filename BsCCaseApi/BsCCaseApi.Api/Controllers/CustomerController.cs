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

    [HttpPatch("{customerId:guid}")]
    public Task<Customer> Update([FromBody] CustomerDto customer, Guid customerId)
    {
        var customerToUpdate = customer.ToCustomer();
        customerToUpdate.Id = customerId;
        return customerService.UpdateCustomer(customerToUpdate);
    }
}