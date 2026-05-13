using BsCCaseApi.Domain.Interfaces;
using BsCOpenSearchSync.DataAccess.Store;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.Client;

public class SyncEventService(EventDbContext eventDbContext, DbContext dbContext) : ISyncEventService
{
    public async Task<T?> DoOperation<T>(SyncType type, object obj) where T : class, IHasId
    {
        var tableName = typeof(T).Name + "s"; // generalization, not great
        
        T? returnObject = null;
        
        switch (type)
        {
            case SyncType.Create:
                returnObject = await Create<T>(obj);
                break;
            case SyncType.Update:
                returnObject = Update<T>(obj);
                break;
            case SyncType.Delete:
                returnObject = await Delete<T>(obj);
                break;
            default:
                return returnObject;
        }

        var @event = new SyncEvent
        {
            Type = type,
            TableName = tableName,
            ObjectId = obj is T objT ? objT.Id : (Guid)obj
        };
        await eventDbContext.SyncEvents.AddAsync(@event);
        await dbContext.SaveChangesAsync();
        return returnObject;
    }
    public async Task AddSyncEvent(SyncEvent syncEvent)
    {
        await dbContext.SaveChangesAsync();
    }

    private async Task<T> Create<T>(object obj) where T : class, IHasId
    {
        if (obj is not T objT)
        {
            throw new Exception("Object must be of type " + typeof(T).Name);
        }
        return (await dbContext.AddAsync(objT)).Entity;
    }

    private T Update<T>(object obj) where T : class, IHasId
    {
        if (obj is not T objT)
        {
            throw new Exception("Object must be of type " + typeof(T).Name);
        }
        return dbContext.Update(objT).Entity;
    }

    private async Task<T> Delete<T>(object obj) where T : class, IHasId
    {
        if (obj is not Guid id)
        {
            throw new Exception("Object must be a Guid");
        }
        // get entity
        var objT = await dbContext.FindAsync<T>(id);
        if (objT is null)
        {
            throw new Exception($"Object with Id {id} does not exist");
        }
        dbContext.Remove(objT);
        return objT;
    }
}