using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Models.Request;

public class CustomerDto
{
    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }

    public Customer ToCustomer()
    {
        return new Customer
        {
            Email = Email,
            Firstname = Firstname,
            Lastname = Lastname
        };
    }
}