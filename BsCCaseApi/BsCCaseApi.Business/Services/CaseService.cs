using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.DataAccess.Services;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;

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
        var created = await dbContext.Cases.AddAsync(@case);

        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = created.Entity.Id,
            TableName = "Cases",
            Type = SyncType.Create
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteCase(int caseId)
    {
        var @case = await dbContext.Cases.FindAsync(caseId);
        if (@case == null)
        {
            throw new Exception($"Case {caseId} not found");
        }
        dbContext.Cases.Remove(@case);
        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = @case.Id,
            TableName = "Cases",
            Type = SyncType.Delete
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<Case> UpdateCase(Case @case)
    {
        var updated = dbContext.Cases.Update(@case);
        await syncEventService.AddSyncEvent(new SyncEvent
        {
            ObjectId = updated.Entity.Id,
            TableName = "Cases",
            Type = SyncType.Update
        });
        await dbContext.SaveChangesAsync();
        return updated.Entity;
    }
}