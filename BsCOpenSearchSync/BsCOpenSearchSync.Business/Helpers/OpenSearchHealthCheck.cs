using Microsoft.Extensions.Logging;
using OpenSearch.Net;

namespace BsCOpenSearchSync.Business.Helpers;

public class OpenSearchHealthCheck(IOpenSearchLowLevelClient client, ILogger<OpenSearchHealthCheck> logger) : IOpenSearchHealthCheck
{
    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PingAsync<StringResponse>(ctx: cancellationToken);
            logger.LogInformation(response.Success ? "OpenSearch is healthy" : "OpenSearch is offline");

            SyncServiceFlags.OpenSearchIsHealthy = response.Success;
            return response.Success;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}