using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;
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
        var created = await dbContext.Cars.AddAsync(car);
        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = created.Entity.Id,
            TableName = "Cars",
            Type = SyncType.Create
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<Car> UpdateCar(Car car)
    {
        var updated = dbContext.Cars.Update(car);
        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = updated.Entity.Id,
            TableName = "Cars",
            Type = SyncType.Update
        });
        await dbContext.SaveChangesAsync();
        return updated.Entity;
    }
}