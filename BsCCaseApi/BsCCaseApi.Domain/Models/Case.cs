using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BsCCaseApi.Domain.enums;
using BsCCaseApi.Domain.Interfaces;

namespace BsCCaseApi.Domain.Models;

public class Case : IHasId
{
    public Guid Id { get; set; } = Guid.NewGuid();
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