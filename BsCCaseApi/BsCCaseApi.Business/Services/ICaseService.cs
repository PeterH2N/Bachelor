using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Business.Services;

public interface ICaseService
{
    public Task<List<Case>> GetAllCases();
    
    public Task<Case> GetCaseById(Guid caseId);
    public Task CreateCase(Case @case);
    public Task DeleteCase(int caseId);
    public Task<Case> UpdateCase(Case @case);
}