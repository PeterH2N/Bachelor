using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BsCCaseApi.Commons.enums;

namespace BsCCaseApi.Library.Models;

public class Case
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string CaseName { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string CaseDescription { get; set; } = string.Empty;
    public CaseType CaseType { get; set; }
    public Customer? Customer { get; set; }
    [JsonIgnore]
    public int CustomerId { get; set; }
    public Employee? Employee { get; set; }
    [JsonIgnore]
    public int EmployeeId { get; set; }
    public Car? Car { get; set; }
    [JsonIgnore]
    public int? CarId { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DateTime CompleteDate { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool Deleted { get; set; }

    public bool Archived { get; set; }
    public DateTime? DeletedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    
}