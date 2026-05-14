using System.Text.Json;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.Business.Helpers;

public class JsonProcessor(DbContext dbContext)
{
    public string BulkJsonFromIds(IEnumerable<SyncEvent> syncEvents)
    {
        var bulkJson  = "";
        foreach (var syncEvent in syncEvents)
        {
            bulkJson += JsonBulkFromId(syncEvent.Type, syncEvent.ObjectId, syncEvent.TableName);
            bulkJson += "\n";
        }
        
        return bulkJson;
    }
    
    public string JsonBulkFromId(SyncType syncType, Guid id, string tableName)
    {
        var type = FindEntityTypeForTable(tableName);
        if (type is null)
        {
            throw new Exception($"Table {tableName} not found");
        }
        var result = FindWithRelationships(type, id);
        if (result is null)
        {
            throw new Exception($"No entity with id {id} found");
        }

        var resJson = JsonSerializer.Serialize(result);
        var actionLine = GetActionLine(syncType, tableName, id);
        
        return actionLine + "\n" + resJson;
    }
    
    private Type? FindEntityTypeForTable(string tableName)
    {
        return dbContext.Model.GetEntityTypes()
            .FirstOrDefault(e => e.GetTableName() == tableName)
            ?.ClrType;
    }
    
    private object? FindWithRelationships(Type entityType, object id)
    {
        var set = (IQueryable)dbContext.GetType()
            .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
            .MakeGenericMethod(entityType)
            .Invoke(dbContext, null)!;

        var entityMeta = dbContext.Model.FindEntityType(entityType)!;
        foreach (var navigation in entityMeta.GetNavigations())
        {
            set = EntityFrameworkQueryableExtensions.Include(
                (IQueryable<object>)set, 
                navigation.Name
            );
        }
        // Filter by primary key
        var keyProperty = entityMeta.FindPrimaryKey()!.Properties[0];
        return set.Cast<object>().FirstOrDefault(e => 
            EF.Property<object>(e, keyProperty.Name).Equals(id));
    }

    private string GetActionLine(SyncType type, string index, Guid id)
    {
        return $"{{ \"{type.ToString().ToLowerInvariant()}\": {{\"_index\": \"{index}\", \"_id\": \"{id}\" }} }}";
    }
}