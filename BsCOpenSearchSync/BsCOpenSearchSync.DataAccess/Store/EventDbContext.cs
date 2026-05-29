using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.DataAccess.Store;

public class EventDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<SyncEvent>  SyncEvents { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}