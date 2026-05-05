using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BsCCaseApi.Library.models;
[Table("Cars")]
public class Car
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public string RegNo { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string VIN { get; set; }
    
    public IEnumerable<Case> Cases { get; } = new List<Case>();
}