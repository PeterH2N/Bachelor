using BsCCaseApi.Library.database;

namespace BsCCaseApi.Library.services;

public class CaseService(AppDbContext context) : ICaseService
{
    private readonly AppDbContext _context = context;
    
    
}