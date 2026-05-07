using BsCCaseApi.Commons.Models;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.Library.Store;

public class CaseDbContext : DbContext
{
    public DbSet<Case> Cases { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Employee> Employees { get; set; }
    
    private readonly string _connectionString = Environment.GetEnvironmentVariable("DB_URL") ?? string.Empty;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            _connectionString
        );
    }
}