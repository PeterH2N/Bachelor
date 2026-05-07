using BsCCaseApi.Commons.Models;

namespace BsCOpenSearchSync.Library.Services;

public interface ISyncService
{
    public Task<Case?> GetCaseById(string id);
}