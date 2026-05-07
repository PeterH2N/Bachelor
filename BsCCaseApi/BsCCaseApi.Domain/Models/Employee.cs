using System.Text.Json.Serialization;

namespace BsCCaseApi.Commons.Models;

public class Employee
{
    public int Id { get; set; }
    public string NameInitials { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    [JsonIgnore]
    public IEnumerable<Case> Cases { get; } = new  List<Case>();
}