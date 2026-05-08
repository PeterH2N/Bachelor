using System.Text.Json.Serialization;

namespace BsCCaseApi.Domain.Models;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NameInitials { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    [JsonIgnore]
    public IEnumerable<Case> Cases { get; } = new  List<Case>();
}