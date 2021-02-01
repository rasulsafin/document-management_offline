using System;

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

        /// <summary>
        /// Start the synchronization process
        /// </summary>
        void StartSync();

        /// <summary>
        /// Астановитесь (Орфография сохранена)
        /// </summary>
        void StopSync();

        /// <summary>
        /// Get information about the synchronization progress
        /// </summary>
        /// <returns>Object of progress</returns>
        ProgressSync GetProgress();
    }

    public struct ProgressSync
    {
        /// <summary>total  - total number of elements found</summary>
        public int total;
        /// <summary>message  - short name of the synchronization stage</summary>
        public string message;
        /// <summary>current - synchronized items at the moment</summary>
        public int current;
        /// <summary>
        /// 
        /// </summary>
        public Exception error;
    }

    public enum TypeChange
    {
        Update,
        Delete,
    }

    public enum TableRevision
    {
        Users,
        Projects,
        Items,
        Objectives,
        ObjectiveTypes,
    }
}
