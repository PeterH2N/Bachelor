
using Bogus;
using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Business.Helpers;

public class ModelFaker(AppDbContext context) : IModelFaker
{
    private readonly Faker<Customer> _customerFaker = new Faker<Customer>()
        .RuleFor(c => c.Email, f => f.Internet.Email())
        .RuleFor(c => c.Firstname, f => f.Name.FirstName())
        .RuleFor(c => c.Lastname, f => f.Name.LastName());
    
    private readonly Faker<Employee> _employeeFaker = new Faker<Employee>()
        .RuleFor(e => e.Firstname, f => f.Name.FirstName())
        .RuleFor(e => e.Lastname, f => f.Name.LastName())
        .RuleFor(e => e.NameInitials, f => f.Random.AlphaNumeric(3));
    
    private readonly Faker<Car> _carFaker = new Faker<Car>()
        .RuleFor(c => c.Model, f => f.Vehicle.Model())
        .RuleFor(c => c.Make, f => f.Vehicle.Manufacturer())
        .RuleFor(c => c.RegNo, f => f.Random.AlphaNumeric(7))
        .RuleFor(c => c.VIN, f => f.Random.AlphaNumeric(17))
        .RuleFor(c => c.Customer, f => f.Random.CollectionItem(context.Customers.ToList()));
    
    private readonly Faker<Case> _caseFaker = new Faker<Case>()
        .RuleFor(c => c.CaseName, f => f.Random.Words(4))
        .RuleFor(c => c.CaseDescription, f => f.Rant.Review())
        .RuleFor(c => c.Employee, f => f.Random.CollectionItem(context.Employees.ToList()))
        .RuleFor(c => c.Car, f => f.Random.CollectionItem(context.Cars.ToList()))
        .RuleFor(c => c.Customer, f => f.Random.CollectionItem(context.Customers.ToList()))
        .RuleFor(c => c.DeliveryDate, (f, c) => f.Date.Past(10))
        .RuleFor(c => c.CompleteDate, (f, c) => f.Date.Future(1, c.DeliveryDate))
        .RuleFor(c => c.BeginTime, (f, c) => f.Date.Between(c.DeliveryDate, c.CompleteDate))
        .RuleFor(c => c.EndTime, (f, c) => f.Date.Between(c.BeginTime, c.CompleteDate))
        .RuleFor(c => c.Deleted, f => f.Random.Bool())
        .RuleFor(c => c.DeletedDate, (f, c) => c.Deleted ? f.Date.Between(c.BeginTime, c.CompleteDate) : null)
        .RuleFor(c => c.ModifiedDate, (f, c) => c.EndTime);
    
    public List<Customer> RandomCustomer(int amount)
    {
        return _customerFaker.Generate(amount);
    }
    
    public List<Employee> RandomEmployee(int amount)
    {
        return _employeeFaker.Generate(amount);
    }
    
    public List<Car> RandomCar(int amount)
    {
        return _carFaker.Generate(amount);
    }
    
    public List<Case> RandomCase(int amount)
    {
        return _caseFaker.Generate(amount);
    }
}