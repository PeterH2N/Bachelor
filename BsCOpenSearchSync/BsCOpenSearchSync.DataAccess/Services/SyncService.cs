using BsCCaseApi.Commons.Models;
using BsCOpenSearchSync.Library.Store;

namespace BsCOpenSearchSync.Library.Services;

public class SyncService(CaseDbContext caseDbContext) : ISyncService
{
    public async Task<Case?> GetCaseById(string id)
    {
        return await caseDbContext.Cases.FindAsync(id);
    }
}