using BsCCaseApi.Business.Helpers;
using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BsCCaseApi.Business.Services;

public class CarService(AppDbContext dbContext, ISyncEventService syncEventService, IModelFaker modelFaker) : ICarService
{
    public async Task<List<Car>> GetAllCars()
    {
        return await dbContext.Cars.Include(c => c.Customer).ToListAsync();
    }

    public async Task<Car> GetCarById(Guid carId)
    {
        var car = await dbContext.Cars.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == carId);
        return car ?? throw new Exception($"Car not found");
    }

    public async Task CreateCar(Car car)
    {
        await syncEventService.DoOperation<Car>(SyncType.Create, car);
    }

    public async Task<Car?> UpdateCar(Car car)
    {
        return await syncEventService.DoOperation<Car>(SyncType.Update, car);
    }

    public async Task<List<Car>> CreateRandomCar(int amount)
    {
        var newCars = modelFaker.RandomCar(amount);
        foreach (var car in newCars)
        {
            await syncEventService.DoOperation<Car>(SyncType.Create, car);
        }
        
        return newCars;
    } 
    
    public async Task<Car> UpdateRandomCar()
    {
        var updatedCar = modelFaker.RandomCar(1).First();
        // get random id
        var randomId = await dbContext.Set<Car>()
            .OrderBy(e => EF.Functions.Random())
            .Select(e => e.Id)
            .FirstOrDefaultAsync();
        updatedCar.Id = randomId;
        
        await syncEventService.DoOperation<Car>(SyncType.Update, updatedCar);
        return updatedCar;
    }
}