using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Manages synchronization
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Notification of an element change
        /// </summary>
        /// <param name="table">table</param>
        /// <param name="id">id element</param>
        /// <param name="type">update / delete</param>
        void Update(TableRevision table, int id, TypeChange type = TypeChange.Update);
    }
}
