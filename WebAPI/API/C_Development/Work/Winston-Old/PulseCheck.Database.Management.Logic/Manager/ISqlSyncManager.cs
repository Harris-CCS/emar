using PulseCheck.Database.Management.Logic.Type;

namespace PulseCheck.Database.Management.Logic.Manager
{
    public interface ISqlSyncManager
    {
        void SyncTableColumns(SyncTableRequest request);
    }
}