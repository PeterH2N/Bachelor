using BsCCaseApi.Commons.Models;

namespace BsCCaseApi.Library.Services;

public interface ICaseService
{
    public Task<Case> GetCaseById(int caseId);
    public Task CreateCase(Case @case);
    public Task DeleteCase(int caseId);
    public Task<Case> UpdateCase(Case @case);
}