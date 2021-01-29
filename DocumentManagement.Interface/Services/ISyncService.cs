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

        ///// <summary>
        ///// Get information about the synchronization progress
        ///// </summary>
        ///// <returns>
        ///// <para>current - synchronized items at the moment</para>
        ///// <para>total  - total number of elements found</para>
        ///// <para>step  - short name of the synchronization stage</para>
        ///// </returns>

        /// <summary>
        /// Get information about the synchronization progress
        /// </summary>
        /// <returns>Object of progress</returns>
        ProgressSync GetProgressSync();
    }

    public struct ProgressSync
    {
        public int total;
        public string message;
        public int current;
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
