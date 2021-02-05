using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Tests
{
    internal class SyncServiceTest : ISyncService
    {
        public void StartSync()
        {
        }

        public void StopSync()
        {
        }

        public void Update(TableRevision table, int id, TypeChange type = TypeChange.Update)
        {
        }

        public ProgressSync GetProgress()
        {
            return default;
        }
    }
}