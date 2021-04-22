using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service for managing Project entities.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Get list of all projects.
        /// </summary>
        /// <returns>List of all projects.</returns>
        Task<IEnumerable<ProjectToListDto>> GetAllProjects();

        /// <summary>
        /// Get projects linked to the specific user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>List of projects.</returns>
        Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID);

        /// <summary>
        /// Create new project.
        /// </summary>
        /// <param name="projectToCreate">Project data.</param>
        /// <returns>Created project.</returns>
        Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate);

        /// <summary>
        /// Delete project by its id.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>True if project was deleted.</returns>
        Task<bool> Remove(ID<ProjectDto> projectID);

        /// <summary>
        /// Update project's values.
        /// </summary>
        /// <param name="project">Project data to update.</param>
        /// <returns>True, if updated successfully.</returns>
        Task<bool> Update(ProjectDto project);

        /// <summary>
        /// Get project by id.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>Found project.</returns>
        Task<ProjectDto> Find(ID<ProjectDto> projectID);

        /// <summary>
        /// Get list of users that have access to this project.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>List of users.</returns>
        Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID);

        /// <summary>
        /// Link existing project to users.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <param name="users">List of user's ids connect project to.</param>
        /// <returns>True if linked successfully.</returns>
        Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users);

        /// <summary>
        /// Unlink existing project from list of users.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <param name="users">List of user's ids unlink project from.</param>
        /// <returns>True if unlinked successfully.</returns>
        Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users);
    }
}
