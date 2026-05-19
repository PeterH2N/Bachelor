using BsCOpenSearchSync.Business.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BsCOpenSearchSync.Business.Helpers;

public class OpenSearchHealthJob(IOpenSearchHealthCheck healthCheck, ISyncService syncService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // if opensearch has been offline and is healthy again
        if (!SyncServiceFlags.OpenSearchIsHealthy && await healthCheck.CheckHealthAsync())
        {
            await syncService.DoAllSyncs();
        }
    }
}