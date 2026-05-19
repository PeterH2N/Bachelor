using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Business.Helpers;

public interface IDbSeeder
{
    public Task SeedData();

}