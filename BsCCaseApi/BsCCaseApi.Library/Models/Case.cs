
using System.ComponentModel.DataAnnotations;

namespace BsCCaseApi.Library.models;

public class Case
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string CaseName { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string CaseDescription { get; set; } = string.Empty;
    public Customer Customer { get; set; }
    public int CustomerId { get; set; }
    
    public Employee Employee { get; set; }
    public int EmployeeId { get; set; }
    
    public int? CarId { get; set; }
    public Car? Car { get; set; }
    
    public DateTime DeliveryDate { get; set; }
    public DateTime CompleteDate { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool Deleted { get; set; }

    public bool Archived { get; set; }
    public DateTime? DeletedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    
}