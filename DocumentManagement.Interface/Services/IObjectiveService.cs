using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service for objectives.
    /// </summary>
    public interface IObjectiveService
    {
        /// <summary>
        /// Get new objective and write in to database.
        /// </summary>
        /// <param name="data">Data for new objective.</param>
        /// <returns>Added objective.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something went wrong.</exception>
        Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data);

        /// <summary>
        /// Delete objectives from database by its id.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>True id objective was deleted.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when objective with that id does not exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Remove(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Update existing objective.
        /// </summary>
        /// <param name="objectiveData">Objective to  update.</param>
        /// <returns>True if updated successfully.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when objective does not exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Update(ObjectiveDto objectiveData);

        /// <summary>
        /// Find and return objective by id if exists.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>Found objective.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when objective with that id does not exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Return list of objectives, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <returns>Collection of objectives.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when project with that id does not exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID);

        /// <summary>
        /// Generate report about selected objectives.
        /// </summary>
        /// <param name="objectives">List of objective id's.</param>
        /// <param name="path">Path to report storage.</param>
        /// <param name="userID">ID of the user, who generates the report.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>Object representing the result of report creation process.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when one of the objectives is not found.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ArgumentValidationException">Thrown when list of objectives is empty.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ObjectiveReportCreationResultDto> GenerateReport(IEnumerable<ID<ObjectiveDto>> objectives, string path, int userID, string projectName);
    }
}
