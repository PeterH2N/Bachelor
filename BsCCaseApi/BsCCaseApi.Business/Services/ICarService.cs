using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Business.Services;

public interface ICarService
{
    public Task<List<Car>> GetAll();
    public Task<Car> GetCarById(Guid carId);
    public Task CreateCar(Car car);
    public Task<Car> UpdateCar(Car car);
}