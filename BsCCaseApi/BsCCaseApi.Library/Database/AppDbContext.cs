using BsCCaseApi.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace BsCCaseApi.Library.Database;

public class AppDbContext : DbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>();
        modelBuilder.Entity<Case>();
        modelBuilder.Entity<Employee>();
        modelBuilder.Entity<Car>();
    }
}