using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Tests
{
    internal class SyncServiceTest : ISyncService
    {
        
        public (int current, int total, string step) GetSyncProgress()
        {
            return (0, 0, string.Empty);
        }

        public void StartSync()
        {
        }

        public void StopSync()
        {
        }

        public void Update(TableRevision table, int id, TypeChange type = TypeChange.Update)
        {
            
        }
    }
}