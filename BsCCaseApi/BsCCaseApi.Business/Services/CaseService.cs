using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.Domain.Enums;

namespace BsCCaseApi.Business.Services;

public class CaseService(AppDbContext dbContext, ISyncEventService syncEventService) : ICaseService
{
    public async Task<Case> GetCaseById(Guid caseId)
    {
        var @case = await dbContext.Cases.FindAsync(caseId);
        return @case ?? throw new Exception("Case not found");
    }

    public async Task CreateCase(Case @case)
    {
        await syncEventService.DoOperation<Case>(SyncType.Create, @case);
    }

    public async Task DeleteCase(int caseId)
    {
        await syncEventService.DoOperation<Case>(SyncType.Delete, caseId);
    }

    public async Task<Case> UpdateCase(Case @case)
    {
        return await syncEventService.DoOperation<Case>(SyncType.Update, @case);
    }
}