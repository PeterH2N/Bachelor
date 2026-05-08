using BsCOpenSearchSync.Domain.Enums;

namespace BsCOpenSearchSync.Domain.Models.Events;

public class SyncEvent
{
    public int Id { get; set; }
    public SyncType Type { get; set; }
    public string TableName { get; set; } = string.Empty;
    public required Guid ObjectId { get; set; }
    public SyncStatus Status { get; set; } = SyncStatus.Waiting;
}