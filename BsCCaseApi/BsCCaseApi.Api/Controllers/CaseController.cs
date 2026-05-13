using BsCCaseApi.Business.Services;
using BsCCaseApi.Domain.Models;
using BsCCaseApi.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;


[Route("api/[controller]/[action]")]
[ApiController]
public class CaseController(ICaseService caseService)
{
    [HttpGet]
    public Task<List<Case>> GetAll()
    {
        return caseService.GetAllCases();
    }

    [HttpGet("{caseId:guid}")]
    public Task<Case> Get(Guid caseId)
    {
        return caseService.GetCaseById(caseId);
    }

    [HttpPut]
    public Task Create([FromBody] CaseDto @case)
    {
        return caseService.CreateCase(@case.ToCase());
    }

    [HttpPatch("{caseId:int}")]
    public Task<Case> Update([FromBody] CaseDto @case, Guid caseId)
    {
        var caseToUpdate = @case.ToCase();
        caseToUpdate.Id = caseId;
        return caseService.UpdateCase(caseToUpdate);
    }

    [HttpDelete("{caseId:int}")]
    public Task Delete(int caseId)
    {
        return caseService.DeleteCase(caseId);
    }
}