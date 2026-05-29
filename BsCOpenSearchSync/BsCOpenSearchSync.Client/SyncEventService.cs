using BsCCaseApi.Domain.Interfaces;
using BsCOpenSearchSync.DataAccess.Store;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BsCOpenSearchSync.Client;

public class SyncEventService(EventDbContext eventDbContext, DbContext dbContext, HttpClient httpClient, ILogger<SyncEventService> logger) : ISyncEventService
{
    public async Task<T?> DoOperation<T>(SyncType type, object obj) where T : class, IHasId
    {
        var tableName = dbContext.Model.FindEntityType(typeof(T))?.GetTableName();
        if (tableName is null)
        {
            throw new Exception("Not a valid type: " + typeof(T).Name);
        }
        
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
        
        // cascade changes if update
        if (type == SyncType.Update)
        {
            var cascadingEvents = GetCascadingSyncEvents(typeof(T), returnObject);
            cascadingEvents = cascadingEvents.DistinctBy(e => new { e.ObjectId, e.TableName });
            await eventDbContext.SyncEvents.AddRangeAsync(cascadingEvents);
        }
        
        
        await eventDbContext.SaveChangesAsync();
        await dbContext.SaveChangesAsync();
        // calls sync endpoint but does not wait for it
        CallSyncEndpoint();
        return returnObject;
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

    // Does not require parameters, call will sync all available events
    private void CallSyncEndpoint()
    {
        const string url = $"api/Sync/doAll";

        _ = Task.Run(async () =>
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Accept", "application/json");
                await httpClient.SendAsync(request);
            }
            catch (HttpRequestException e)
            {
                logger.LogError(e, "Error during sync endpoint");
            }
        });
    }
    
    public IEnumerable<SyncEvent> GetCascadingSyncEvents(Type changedType, object changedEntity)
    {
        var changedEntityMeta = dbContext.Model.FindEntityType(changedType);
        if (changedEntityMeta is null) yield break;

        var changedId = changedEntityMeta.FindPrimaryKey()!.Properties[0]
            .PropertyInfo!.GetValue(changedEntity);

        // find all entity types that have a FK pointing to the changed type
        var dependentTypes = dbContext.Model.GetEntityTypes()
            .Where(e => e.GetForeignKeys()
                .Any(fk => fk.PrincipalEntityType.ClrType == changedType));

        foreach (var dependentType in dependentTypes)
        {
            var fk = dependentType.GetForeignKeys()
                .First(fk => fk.PrincipalEntityType.ClrType == changedType);

            var fkPropertyName = fk.Properties[0].Name;
            var pkPropertyName = dependentType.FindPrimaryKey()!.Properties[0].Name;
            var tableName = dependentType.GetTableName()!;

            // find all dependent entities where FK matches the changed entity's PK
            var dependentEntities = dbContext.Set<object>()
                .Cast<object>()
                .AsEnumerable()
                .Where(e => e.GetType() == dependentType.ClrType &&
                            e.GetType().GetProperty(fkPropertyName)?.GetValue(e)?.Equals(changedId) == true);

            foreach (var dependent in dependentEntities)
            {
                var dependentId = dependent.GetType().GetProperty(pkPropertyName)?.GetValue(dependent);
                if (dependentId is null) continue;

                yield return new SyncEvent
                {
                    ObjectId = (Guid)dependentId,
                    TableName = tableName,
                    Type = SyncType.Update,
                    Status = SyncStatus.Waiting
                };
            }
        }
    }
}