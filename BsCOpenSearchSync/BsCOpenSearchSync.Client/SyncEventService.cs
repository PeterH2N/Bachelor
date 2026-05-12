using BsCOpenSearchSync.DataAccess.Store;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.Client;

public class SyncEventService(EventDbContext eventDbContext, DbContext dbContext) : ISyncEventService
{
    public async Task<T> DoOperation<T>(SyncType type, Object obj)
    {
        string tableName = typeof(T).Name + "s"; // generalization, not great
        switch (type)
        {
            case SyncType.Create:
                if (obj is not T)
                {
                    throw new Exception("Object must be of type " + typeof(T).Name);
                }
                await dbContext.AddAsync(obj);
                break;
            case SyncType.Update:
                if (obj is not T)
                {
                    throw new Exception("Object must be of type " + typeof(T).Name);
                }
                dbContext.Update((T)obj);
                break;
        }

        var @event = new SyncEvent
        {
            Type = type,
            TableName = tableName,
            ObjectId = Guid.NewGuid(),
        };
        await eventDbContext.SyncEvents.AddAsync(@event);
        await dbContext.SaveChangesAsync();
        return (T)obj;
    }
    public async Task AddSyncEvent(SyncEvent syncEvent)
    {
        await dbContext.SaveChangesAsync();
    }
}