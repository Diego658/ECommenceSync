using System;

namespace ECommenceSync
{
    public class SyncTimeInfo
    {
        public DateTimeOffset LastSyncTime { get; set; }
        public DateTimeOffset CurrentSyncTime { get; set; }
    }
}
