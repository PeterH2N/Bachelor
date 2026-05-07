using BsCCaseApi.Commons.Models;
using BsCCaseApi.Library.Services;
using BsCCaseApi.Models;
using BsCCaseApi.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;


[Route("api/[controller]/[action]")]
[ApiController]
public class CaseController(ICaseService caseService)
{

    [HttpGet("{caseId:int}")]
    public Task<Case> Get(int caseId)
    {
        return caseService.GetCaseById(caseId);
    }

    [HttpPut]
    public Task Create([FromBody] CaseDto @case)
    {
        return caseService.CreateCase(@case.ToCase());
    }

    [HttpPatch]
    public Task<Case> Update([FromBody] CaseDto @case)
    {
        return caseService.UpdateCase(@case.ToCase());
    }

    [HttpDelete("{caseId:int}")]
    public Task Delete(int caseId)
    {
        return caseService.DeleteCase(caseId);
    }
}