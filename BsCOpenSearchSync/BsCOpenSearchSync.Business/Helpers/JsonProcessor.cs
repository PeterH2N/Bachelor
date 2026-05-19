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
    public static bool JsonEqual(JsonElement a, JsonElement b)
    {
        if (a.ValueKind != b.ValueKind) return false;

        switch (a.ValueKind)
        {
            case JsonValueKind.Object:
                var aProps = a.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                var bProps = b.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

                if (aProps.Count != bProps.Count) return false;

                return aProps.All(kvp =>
                    bProps.TryGetValue(kvp.Key, out var bVal) && JsonEqual(kvp.Value, bVal));

            case JsonValueKind.Array:
                var aArr = a.EnumerateArray().ToList();
                var bArr = b.EnumerateArray().ToList();

                if (aArr.Count != bArr.Count) return false;

                return aArr.Zip(bArr).All(pair => JsonEqual(pair.First, pair.Second));

            case JsonValueKind.String:
                return a.GetString() == b.GetString();

            case JsonValueKind.Number:
                return a.GetDecimal() == b.GetDecimal();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return a.GetBoolean() == b.GetBoolean();

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return true;

            default:
                return false;
        }
    }
    
    private string JsonBulkFromId(SyncType syncType, Guid id, string tableName)
    {
        var type = FindEntityTypeForTable(tableName);
        if (type is null)
        {
            throw new Exception($"Table {tableName} not found");
        }
        
        var actionLine = GetActionLine(syncType, tableName, id);
        
        // if deleting, we should not try to find the object
        if (syncType == SyncType.Delete)
        {
            return actionLine + "\n";
        }
        
        var result = FindWithRelationships(type, id);
        if (result is null)
        {
            throw new Exception($"No entity with id {id} found");
        }

        var resJson = JsonSerializer.Serialize(result);

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

    private static string GetActionLine(SyncType type, string index, Guid id)
    {
        return $"{{ \"{type.ToString().ToLowerInvariant()}\": {{\"_index\": \"{index.ToLowerInvariant()}\", \"_id\": \"{id}\" }} }}";
    }
}