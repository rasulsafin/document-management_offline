using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizer.Models
{
    public class SyncProject
    {
        public ProjectDto dto { get; set; }
        public ulong Rev { get; set; }
    }

    public class SyncUser
    {
        public UserDto dto { get; set; }
        public ulong Rev { get; set; }
    }

    public class SyncItem
    {
        public ItemDto dto { get; set; }
        public int IdProj { get; set; }
        public int IdObj { get; set; }
        public ulong Rev { get; set; }
    }

    public class SyncObjective
    {
        public ObjectiveDto dto { get; set; }
        public int IdProj { get; set; }        
        public ulong Rev { get; set; }
    }
}
