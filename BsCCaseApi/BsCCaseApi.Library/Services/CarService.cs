using BsCCaseApi.Library.Database;
using BsCCaseApi.Library.Models;

namespace BsCCaseApi.Library.Services;

public class CarService(AppDbContext dbContext) : ICarService
{
    public async Task<Car> GetCarById(int carId)
    {
        var car = await dbContext.Cars.FindAsync(carId);
        return car ?? throw new Exception($"Car not found");
    }

    public async Task CreateCar(Car car)
    {
        await dbContext.Cars.AddAsync(car);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Car> UpdateCar(Car car)
    {
        var updated = dbContext.Cars.Update(car);
        await dbContext.SaveChangesAsync();
        return updated.Entity;
    }
}