using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BsCCaseApi.Domain.Interfaces;

namespace BsCCaseApi.Domain.Models;

public class Car : IHasId
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CustomerId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    public Customer? Customer { get; set; }
    public string RegNo { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string VIN { get; set; }
    [JsonIgnore]
    public IEnumerable<Case> Cases { get; } = new List<Case>();
}