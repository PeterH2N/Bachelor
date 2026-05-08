using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.DataAccess.Store;

public class EventDbContext : DbContext
{
    public DbSet<SyncEvent>  SyncEvents { get; set; }
    public DbSet<LatestSync> LatestSyncs { get; set; }
    
    private readonly string _connectionString = Environment.GetEnvironmentVariable("DB_URL") ?? string.Empty;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            _connectionString
        );
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LatestSync>()
            .HasData(new LatestSync{ Value = 0 });
    }
}