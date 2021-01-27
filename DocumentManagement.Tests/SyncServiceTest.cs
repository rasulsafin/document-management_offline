using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Tests
{
    internal class SyncServiceTest : ISyncService
    {
        public void AddChange(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {
        }

        public void AddChange(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
        }

        public void AddChange(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {
        }

        public void AddChange(ID<ProjectDto> id)
        {
        }

        public void AddChange(ID<UserDto> id)
        {
        }

        public (int current, int total, string step) GetSyncProgress()
        {
            return (0, 0, string.Empty);
        }

        public void StartSyncAsync()
        {
        }

        public void StopSyncAsync()
        {
        }
    }
}