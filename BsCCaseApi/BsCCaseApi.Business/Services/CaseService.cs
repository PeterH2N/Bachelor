using BsCCaseApi.Business.Helpers;
using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BsCCaseApi.Business.Services;

public class CaseService(AppDbContext dbContext, ISyncEventService syncEventService, IModelFaker modelFaker) : ICaseService
{
    public async Task<List<Case>> GetAllCases()
    {
        return await dbContext.Cases
            .Include(c => c.Customer)
            .Include(c => c.Car)
            .Include(c=> c.Employee)
            .ToListAsync();
    }
    public async Task<Case> GetCaseById(Guid caseId)
    {
        var @case = await dbContext.Cases
            .Include(c => c.Customer)
            .Include(c => c.Car)
            .Include(c=> c.Employee)
            .FirstOrDefaultAsync(c => c.Id == caseId);
        return @case ?? throw new Exception("Case not found");
    }

    public async Task CreateCase(Case @case)
    {
        await syncEventService.DoOperation<Case>(SyncType.Create, @case);
    }

    public async Task DeleteCase(Guid caseId)
    {
        await syncEventService.DoOperation<Case>(SyncType.Delete, caseId);
    }

    public async Task<Case> UpdateCase(Case @case)
    {
        return await syncEventService.DoOperation<Case>(SyncType.Update, @case);
    }

    public async Task<List<Case>> CreateRandomCase(int amount)
    {
        var newCases = modelFaker.RandomCase(amount);
        foreach (var newCase in newCases)
        {
            await syncEventService.DoOperation<Case>(SyncType.Create, newCase);
        }
        return newCases;
    }
}