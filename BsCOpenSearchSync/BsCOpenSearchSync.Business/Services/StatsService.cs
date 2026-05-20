using System.Text.Json;
using BsCOpenSearchSync.Business.Helpers;
using Microsoft.EntityFrameworkCore;
using OpenSearch.Net;
using HttpMethod = OpenSearch.Net.HttpMethod;

namespace BsCOpenSearchSync.Business.Services;

public class StatsService(IOpenSearchLowLevelClient openSearchClient, DbContext dbContext) : IStatsService
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
            sets[entityType] = JsonProcessor.GetWithRelationships(entityType, dbContext);
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
            var guid = Guid.Parse(id);
            // remove id to track any ids that should be removed from OpenSearch
            ids.Remove(guid);
            
            var source = sources.FirstOrDefault(s => s.GetProperty("Id").ToString() == id);
            
            var objElem = JsonSerializer.SerializeToElement(obj);
            
            if (!JsonProcessor.JsonEqual(source, objElem))
            {
                deltaList.Add(new Tuple<string, Guid>("Index", guid));
            }
            
        }
        
        deltaList.AddRange(ids.Select(id => new Tuple<string, Guid>("Delete", id)));

        return deltaList;
    }
}