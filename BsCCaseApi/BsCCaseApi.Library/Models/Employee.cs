namespace BsCCaseApi.Library.models;

public class Employee
{
    public int Id { get; set; }
    public string NameInitials { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    
    public IEnumerable<Case> Cases { get; } = new  List<Case>();
}