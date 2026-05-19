using Bogus;
using BsCCaseApi.Business.Services;
using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Business.Helpers;

public class DbSeeder(AppDbContext context, IModelFaker modelFaker, ICarService carService, IEmployeeService employeeService, ICaseService caseService, ICustomerService customerService): IDbSeeder
{
    public async Task SeedData()
    {
        Randomizer.Seed = new Random(1337);
        
        if (!context.Customers.Any())
        {
            var customers = modelFaker.RandomCustomer(20);
            foreach (var customer in customers)
            {
                await  customerService.CreateCustomer(customer);
            }
        }

        if (!context.Employees.Any())
        {
            var employees = modelFaker.RandomEmployee(10);
            foreach (var employee in employees)
            {
                await employeeService.CreateEmployee(employee);
            }
        }

        if (!context.Cars.Any())
        {
            var cars = modelFaker.RandomCar(30);
            foreach (var car in cars)
            {
                await carService.CreateCar(car);
            }
        }

        if (!context.Cases.Any())
        {
            var cases = modelFaker.RandomCase(20);
            foreach (var @case in cases)
            {
                await caseService.CreateCase(@case);
            }
        }
    }
}