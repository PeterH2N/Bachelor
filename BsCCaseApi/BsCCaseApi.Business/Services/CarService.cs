using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BsCCaseApi.Business.Services;

public class CarService(AppDbContext dbContext, ISyncEventService syncEventService) : ICarService
{
    public async Task<List<Car>> GetAll()
    {
        return await dbContext.Cars.ToListAsync();
    }

    public async Task<Car> GetCarById(Guid carId)
    {
        var car = await dbContext.Cars.FindAsync(carId);
        return car ?? throw new Exception($"Car not found");
    }

    public async Task CreateCar(Car car)
    {
        await syncEventService.DoOperation<Car>(SyncType.Create, car);
    }

    public async Task<Car> UpdateCar(Car car)
    {
        return await syncEventService.DoOperation<Car>(SyncType.Update, car);
    }
}