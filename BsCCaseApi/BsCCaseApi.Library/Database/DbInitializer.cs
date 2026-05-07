using Bogus;
using BsCCaseApi.Library.Models;

namespace BsCCaseApi.Library.Database;

public static class DbInitializer
{
    public static async Task SeedData(AppDbContext context)
    {
        Randomizer.Seed = new Random(1337);

        List<Customer>? dbCustomers = null;
        List<Employee>? dbEmployees = null;
        List<Car>? dbCars = null;
        
        if (!context.Customers.Any())
        {
            var faker = new Faker<Customer>()
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Firstname, f => f.Name.FirstName())
                .RuleFor(c => c.Lastname, f => f.Name.LastName());
            
            var customers = faker.Generate(100);
            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
            dbCustomers = context.Customers.ToList();
        }

        if (!context.Employees.Any())
        {
            var faker = new Faker<Employee>()
                .RuleFor(e => e.Firstname, f => f.Name.FirstName())
                .RuleFor(e => e.Lastname, f => f.Name.LastName())
                .RuleFor(e => e.NameInitials, f => f.Random.AlphaNumeric(3));

            var employees = faker.Generate(10);
            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();
            dbEmployees = context.Employees.ToList();
        }

        if (!context.Cars.Any())
        {
            var faker = new Faker<Car>()
                .RuleFor(c => c.Model, f => f.Vehicle.Model())
                .RuleFor(c => c.Make, f => f.Vehicle.Manufacturer())
                .RuleFor(c => c.RegNo, f => f.Random.AlphaNumeric(7))
                .RuleFor(c => c.VIN, f => f.Random.AlphaNumeric(17))
                .RuleFor(c => c.Customer, f => f.Random.CollectionItem(dbCustomers));
            
            var cars = faker.Generate(50);
            context.Cars.AddRange(cars);
            await context.SaveChangesAsync();
            dbCars = context.Cars.ToList();
        }

        if (!context.Cases.Any())
        {
            var faker = new Faker<Case>()
                .RuleFor(c => c.CaseName, f => f.Random.Words(4))
                .RuleFor(c => c.CaseDescription, f => f.Rant.Review())
                .RuleFor(c => c.Employee, f => f.Random.CollectionItem(dbEmployees))
                .RuleFor(c => c.Car, f => f.Random.CollectionItem(dbCars))
                .RuleFor(c => c.Customer, (f, c) => c.Car!.Customer)
                .RuleFor(c => c.DeliveryDate, (f, c) => f.Date.Past(10))
                .RuleFor(c => c.CompleteDate, (f, c) => f.Date.Future(1, c.DeliveryDate))
                .RuleFor(c => c.BeginTime, (f, c) => f.Date.Between(c.DeliveryDate, c.CompleteDate))
                .RuleFor(c => c.EndTime, (f, c) => f.Date.Between(c.BeginTime, c.CompleteDate))
                .RuleFor(c => c.Deleted, f => f.Random.Bool())
                .RuleFor(c => c.DeletedDate, (f, c) => c.Deleted ? f.Date.Between(c.BeginTime, c.CompleteDate) : null)
                .RuleFor(c => c.ModifiedDate, (f, c) => c.EndTime);
            
            var cases = faker.Generate(100);
            context.Cases.AddRange(cases);
            await context.SaveChangesAsync();
        }
    }
}