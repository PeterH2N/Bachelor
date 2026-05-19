using System.Text.Json;
using BsCCaseApi.DataAccess.Store;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Dictionary<Type, IEnumerable<Guid>>> GetDelta()
    {
        var delta = new Dictionary<Type, IEnumerable<Guid>>();
        
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
            delta[entityType] = await GetDeltaFor(entityType, set);
        }
        
        return delta;
    }

    private async Task<IEnumerable<Guid>> GetDeltaFor(Type entityType, IQueryable set)
    {
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
        
        

        return new List<Guid>();
    }

    private IQueryable GetWithRelationships(Type entityType)
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

        return set;
    }
}