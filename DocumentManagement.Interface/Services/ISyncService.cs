using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Manages synchronization
    /// </summary>
    public interface ISyncService
    {

        /// <summary>
        /// Notifying the system about object changes
        /// </summary>
        /// <param name="id">Item identifier</param>
        /// <param name="idObj">ID of the Objective where Item is located</param>
        /// <param name="idProj">ID of the project where Objective is located</param>
        void AddChange(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj);

        /// <summary>
        /// Notifying the system about object changes
        /// </summary>
        /// <param name="id">Item identifier</param>
        /// <param name="idProj">ID of the project where Item is located</param>
        void AddChange(ID<ItemDto> id, ID<ProjectDto> idProj);

        /// <summary>
        /// Notifying the system about object changes
        /// </summary>
        /// <param name="id">Objective identifier</param>
        /// <param name="idProj">ID of the project where Objective is located</param>
        void AddChange(ID<ObjectiveDto> id, ID<ProjectDto> idProj);

        /// <summary>
        /// Notifying the system about object changes
        /// </summary>
        /// <param name="id">Project identifier</param>
        void AddChange(ID<ProjectDto> id);

        /// <summary>
        /// Notifying the system about object changes
        /// </summary>
        /// <param name="id">user identifier</param>
        void AddChange(ID<UserDto> id);

        /// <summary>
        /// Start the synchronization process
        /// </summary>
        void StartSyncAsync();
        /// <summary>
        /// Астановись
        /// </summary>
        void StopSyncAsync();

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
}
