using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.General;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Filters;

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
        Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data);

        /// <summary>
        /// Delete objectives from database by its id.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>True id objective was deleted.</returns>
        Task<bool> Remove(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Update existing objective.
        /// </summary>
        /// <param name="objectiveData">Objective to  update.</param>
        /// <returns>True if updated successfully.</returns>
        Task<bool> Update(ObjectiveDto objectiveData);

        /// <summary>
        /// Find and return objective by id if exists.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>Found objective.</returns>
        Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Return list of objectives, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <returns>Collection of objectives.</returns>
        Task<PagedList<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID, ObjectiveFilterParameters filter);

        /// <summary>
        /// Generate report about selected objectives.
        /// </summary>
        /// <param name="objectives">List of objective id's.</param>
        /// <param name="path">Path to report storage.</param>
        /// <param name="userID">ID of the user, who generates the report.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>Object representing the result of report creation process.</returns>
        Task<ObjectiveReportCreationResultDto> GenerateReport(IEnumerable<ID<ObjectiveDto>> objectives, string path, int userID, string projectName);
    }
}
