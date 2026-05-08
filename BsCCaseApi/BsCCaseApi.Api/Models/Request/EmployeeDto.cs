using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Models.Request;

public class EmployeeDto
{
    public string NameInitials { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }

    public Employee ToEmployee()
    {
        return new Employee
        {
            Firstname = Firstname,
            Lastname = Lastname,
            NameInitials = NameInitials
        };
    }
}