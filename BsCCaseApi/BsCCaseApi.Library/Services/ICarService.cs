using BsCCaseApi.Library.Models;

namespace BsCCaseApi.Library.Services;

public interface ICarService
{
    public Task<Car> GetCarById(int carId);
    public Task CreateCar(Car car);
    public Task<Car> UpdateCar(Car car);
}