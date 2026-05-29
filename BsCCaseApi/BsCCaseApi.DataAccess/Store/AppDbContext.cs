using BsCCaseApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BsCCaseApi.DataAccess.Store;

public class AppDbContext : DbContext
{
    public DbSet<Case> Cases { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Employee> Employees { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Cars)
            .WithOne(c => c.Customer)
            .HasForeignKey(c => c.CustomerId);
        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Cases)
            .WithOne(c => c.Customer)
            .HasForeignKey(c => c.CustomerId);
            
        
        modelBuilder.Entity<Case>()
            .HasOne(c => c.Customer)
            .WithMany(c => c.Cases)
            .HasForeignKey(c => c.CustomerId);
        modelBuilder.Entity<Case>()
            .HasOne(c => c.Car)
            .WithMany(c => c.Cases)
            .HasForeignKey(c => c.CarId);
        modelBuilder.Entity<Case>()
            .HasOne(c => c.Employee)
            .WithMany(e => e.Cases)
            .HasForeignKey(c => c.EmployeeId);
        
        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Cases)
            .WithOne(c => c.Employee)
            .HasForeignKey(c => c.EmployeeId);
        
        modelBuilder.Entity<Car>()
            .HasOne(c => c.Customer)
            .WithMany(c => c.Cars)
            .HasForeignKey(c => c.CustomerId);
    }
}