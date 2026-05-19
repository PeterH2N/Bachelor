using System.Text.Json;
using BsCCaseApi.DataAccess.Store;
using BsCOpenSearchSync.Business.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OpenSearch.Net;
using HttpMethod = OpenSearch.Net.HttpMethod;

namespace BsCOpenSearchSync.Business.Services;

public class StatsService(IOpenSearchLowLevelClient openSearchClient, AppDbContext dbContext) : IStatsService
{
    public async Task<string> GetCount(string index)
    {
        var res = await openSearchClient.DoRequestAsync<StringResponse>(HttpMethod.GET,$"{index}/_count", CancellationToken.None);
        
        return res.Body;
    }

    public async Task<Dictionary<string, IEnumerable<Tuple<string, Guid>>>> GetDelta()
    {
        var delta = new Dictionary<string, IEnumerable<Tuple<string, Guid>>>();
        
        // Get all DbSet types
        var entityTypes = dbContext.Model.GetEntityTypes()
            .Select(t => t.ClrType)
            .ToList();
        
        var sets = new Dictionary<Type, IQueryable>();

        foreach (var entityType in entityTypes)
        {
            sets[entityType] = GetWithRelationships(entityType);
        }

        foreach (var (entityType, set) in sets)
        {
            delta[entityType.Name] = await GetDeltaFor(entityType, set);
        }
        
        return delta;
    }

    private async Task<IEnumerable<Tuple<string, Guid>>> GetDeltaFor(Type entityType, IQueryable set)
    {
        var deltaList = new List<Tuple<string, Guid>>();
        var index = dbContext.Model.FindEntityType(entityType)?.GetTableName()?.ToLowerInvariant();
        
        var response = await openSearchClient.SearchAsync<StringResponse>(
            index,
            PostData.Serializable(new
            {
                query = new { match_all = new { } },
                size = 10000
            })
        );

        if (!response.Success)
        {
            throw new Exception("No response from openSearch");
        }
        
        var json = response.Body; // raw JSON string
        // deserialize as needed, e.g. with System.Text.Json:
        var doc = JsonDocument.Parse(json);
        
        var sources = doc.RootElement
            .GetProperty("hits")
            .GetProperty("hits")
            .EnumerateArray()
            .Select(hit => hit.GetProperty("_source"))
            .ToList(); // List<JsonElement>
        
        // list of all IDs in opensearch
        var ids = sources.Select(source => Guid.Parse(source.GetProperty("Id").ToString())).ToList();
        // loop through db objects
        foreach (var obj in set)
        {
            var id = entityType.GetProperty("Id")?.GetValue(obj)?.ToString();
            
            var source = sources.FirstOrDefault(s => s.GetProperty("Id").ToString() == id);

            var objElem = JsonSerializer.SerializeToElement(obj);

            var guid = Guid.Parse(id);
            
            if (!JsonProcessor.JsonEqual(source, objElem))
            {
                deltaList.Add(new Tuple<string, Guid>("Index", guid));
            }
            else
            {
                // remove, to track if there are documents in OpenSearch that should be removed
                ids.Remove(guid);
            }
        }
        
        deltaList.AddRange(ids.Select(id => new Tuple<string, Guid>("Delete", id)));

        return deltaList;
    }

    private IQueryable GetWithRelationships(Type entityType)
    {
        var set = (IQueryable)dbContext.GetType()
            .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
            .MakeGenericMethod(entityType)
            .Invoke(dbContext, null)!;

        var entityMeta = dbContext.Model.FindEntityType(entityType)!;

        foreach (var path in GetIncludePaths(entityMeta))
        {
            set = EntityFrameworkQueryableExtensions.Include(
                (IQueryable<object>)set,
                path
            );
        }

        return set;
    }

    private static IEnumerable<string> GetIncludePaths(IEntityType entityType, string prefix = "", int maxDepth = 3, HashSet<IEntityType>? visited = null)
    {
        visited ??= new HashSet<IEntityType>();
    
        if (maxDepth == 0) yield break;
        if (!visited.Add(entityType)) yield break; // already visiting this type, skip to avoid cycles

        foreach (var navigation in entityType.GetNavigations())
        {
            var path = string.IsNullOrEmpty(prefix)
                ? navigation.Name
                : $"{prefix}.{navigation.Name}";

            yield return path;

            var targetType = navigation.TargetEntityType;
            foreach (var nestedPath in GetIncludePaths(targetType, path, maxDepth - 1, visited))
                yield return nestedPath;
        }
    
        visited.Remove(entityType); // remove so it can be visited on other branches
    }
}