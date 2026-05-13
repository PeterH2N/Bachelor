using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.Business.Helpers;

public class JsonProcessor(DbContext dbContext)
{
    public string JsonBulkFromId(Guid id, string tableName)
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

        return JsonSerializer.Serialize(result);
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
}