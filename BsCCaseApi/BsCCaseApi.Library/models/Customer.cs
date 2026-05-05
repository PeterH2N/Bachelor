namespace BsCCaseApi.Library.models;

public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    
    public IEnumerable<Car> Cars { get; } = new List<Car>();
    public IEnumerable<Case> Cases { get; } = new List<Case>();
}