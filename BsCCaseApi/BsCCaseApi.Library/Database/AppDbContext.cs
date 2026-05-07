using BsCCaseApi.Library.models;
using Microsoft.EntityFrameworkCore;

namespace BsCCaseApi.Library.database;

public class AppDbContext : DbContext
{
    public DbSet<Case> Cases { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Employee> Employees { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=localhost\sqlserver,1433;Database=msdb;Trusted_Connection=True;User Id=sa;Password=Qyy49akg;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>();
        modelBuilder.Entity<Case>();
        modelBuilder.Entity<Employee>();
        modelBuilder.Entity<Car>();
    }
}