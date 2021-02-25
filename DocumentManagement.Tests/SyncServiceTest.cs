using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Synchronizer.Legacy;

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

        public void Update(NameTypeRevision table, int id, TypeChange type = TypeChange.Update)
        {
        }

        public ProgressSync GetProgress()
        {
            return default;
        }
    }
}