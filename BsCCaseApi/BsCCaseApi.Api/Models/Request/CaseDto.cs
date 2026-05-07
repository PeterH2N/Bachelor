using System.ComponentModel.DataAnnotations;
using BsCCaseApi.Commons.enums;
using BsCCaseApi.Library.Models;

namespace BsCCaseApi.Models.Request;

public class CaseDto
{
    public int Id { get; set; }
    public string CaseName { get; set; } = string.Empty;
    public string CaseDescription { get; set; } = string.Empty;
    public CaseType CaseType { get; set; }
    public int CustomerId { get; set; }
    
    public int EmployeeId { get; set; }
    
    public int? CarId { get; set; }
    
    public DateTime DeliveryDate { get; set; }
    public DateTime CompleteDate { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool Deleted { get; set; }

    public bool Archived { get; set; }
    public DateTime? DeletedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public Case ToCase()
    {
        return new Case
        {
            Id = Id,
            CaseName = CaseName,
            CaseDescription = CaseDescription,
            CustomerId = CustomerId,
            EmployeeId = EmployeeId,
            CarId = CarId,
            DeliveryDate = DeliveryDate,
            CompleteDate = CompleteDate,
            BeginTime = BeginTime,
            EndTime = EndTime,
            Deleted = Deleted,
            Archived = Archived,
            DeletedDate = DeletedDate,
            ModifiedDate = ModifiedDate
        };
    }
}