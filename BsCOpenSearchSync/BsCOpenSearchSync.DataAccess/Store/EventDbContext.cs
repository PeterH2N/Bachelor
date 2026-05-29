using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.DataAccess.Store;

public class EventDbContext : DbContext
{
    public DbSet<SyncEvent>  SyncEvents { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}