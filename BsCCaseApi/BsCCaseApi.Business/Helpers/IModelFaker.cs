using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Business.Helpers;

public interface IModelFaker
{
    public List<Customer> RandomCustomer(int amount);
    public List<Employee> RandomEmployee(int amount);
    public List<Car> RandomCar(int amount);
    public List<Case> RandomCase(int amount);
}