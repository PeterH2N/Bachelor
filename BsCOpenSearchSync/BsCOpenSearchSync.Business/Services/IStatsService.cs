using OpenSearch.Net;

namespace BsCOpenSearchSync.Business.Services;

public interface IStatsService
{
    public Task<string> GetCount(string index);
    public Task<Dictionary<string, IEnumerable<Tuple<string, Guid>>>> GetDelta();
}