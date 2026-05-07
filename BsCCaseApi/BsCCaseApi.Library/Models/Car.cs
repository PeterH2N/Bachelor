using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BsCCaseApi.Library.Models;
[Table("Cars")]
public class Car
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    public Customer? Customer { get; set; }
    public string RegNo { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string VIN { get; set; }
    [JsonIgnore]
    public IEnumerable<Case> Cases { get; } = new List<Case>();
}