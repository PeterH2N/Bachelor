using OpenSearch.Net;

namespace BsCOpenSearchSync.Business.Services;

public interface IStatsService
{
    public Task<string> GetCount(string index);
    public Task<Dictionary<Type, IEnumerable<Guid>>> GetDelta();
}