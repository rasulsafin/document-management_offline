using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Manages synchronization
    /// </summary>
    public interface ISyncService
    {

        void Update(TableRevision table, int id, TypeChange type = TypeChange.Update);

        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">Item identifier</param>
        ///// <param name="idObj">ID of the Objective where Item is located</param>
        // void AddChange(ID<ItemDto> id, ID<ObjectiveDto> idObj);
        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">Item identifier</param>
        ///// <param name="idObj">ID of the Objective where Item is located</param>
        // void Delete(ID<ItemDto> id, ID<ObjectiveDto> idObj);
        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">Item identifier</param>
        ///// <param name="idProj">ID of the project where Item is located</param>
        // void AddChange(ID<ItemDto> id, ID<ProjectDto> idProj);
        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">Item identifier</param>
        ///// <param name="idProj">ID of the project where Item is located</param>
        // void Delete(ID<ItemDto> id, ID<ProjectDto> idProj);
        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">Objective identifier</param>
        ///// <param name="idProj">ID of the project where Objective is located</param>
        // void AddChange(ID<ObjectiveDto> id, ID<ProjectDto> idProj);
        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">Objective identifier</param>
        ///// <param name="idProj">ID of the project where Objective is located</param>
        // void Delete(ID<ObjectiveDto> id, ID<ProjectDto> idProj);
        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">Project identifier</param>
        // void AddChange(ID<ProjectDto> id);
        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">Project identifier</param>
        // void Delete(ID<ProjectDto> id);
                ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">user identifier</param>
        // void AddChange(ID<UserDto> id);
        ///// <summary>
        ///// Notifying the system about object changes
        ///// </summary>
        ///// <param name="id">user identifier</param>
        // void Delete(ID<UserDto> id);

        /// <summary>
        /// Start the synchronization process
        /// </summary>
        void StartSync();
        /// <summary>
        /// Астановись
        /// </summary>
        void StopSync();

        /// <summary>
        /// Get information about the synchronization progress
        /// </summary>
        /// <returns>
        /// <para>current - synchronized items at the moment</para>
        /// <para>total  - total number of elements found</para>
        /// <para>step  - short name of the synchronization stage</para>
        /// </returns>
        (int current, int total, string step)  GetSyncProgress();
    }

    public enum TypeChange
    {
        Update,
        Delete
    }

    public enum TableRevision
    {
        Users,
        Projects,
        Items,
        Objectives
    }
}
