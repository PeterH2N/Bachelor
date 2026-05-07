using BsCCaseApi.Commons.Models;
using BsCCaseApi.Library.Store;

namespace BsCCaseApi.Library.Services;

public class CaseService(AppDbContext dbContext) : ICaseService
{
    public async Task<Case> GetCaseById(int caseId)
    {
        var @case = await dbContext.Cases.FindAsync(caseId);
        return @case ?? throw new Exception("Case not found");
    }

    public async Task CreateCase(Case @case)
    {
        await dbContext.Cases.AddAsync(@case);
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
        await dbContext.SaveChangesAsync();
    }

    public async Task<Case> UpdateCase(Case @case)
    {
        var updated = dbContext.Cases.Update(@case);
        await dbContext.SaveChangesAsync();
        return updated.Entity;
    }
}