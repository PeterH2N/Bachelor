using System.Text.Json.Serialization;

namespace BsCCaseApi.Domain.Models;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    [JsonIgnore]
    public IEnumerable<Car> Cars { get; } = new List<Car>();
    [JsonIgnore]
    public IEnumerable<Case> Cases { get; } = new List<Case>();
}