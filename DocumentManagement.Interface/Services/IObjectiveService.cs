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
        /// Gets new objective and writes in to database.
        /// </summary>
        /// <param name="data">Data for new objective.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data);

        /// <summary>
        /// Deletes objetives from database by its id.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Remove(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Updates existing objective.
        /// </summary>
        /// <param name="objectiveData">Objective to  update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Update(ObjectiveDto objectiveData);

        /// <summary>
        /// Finds and returns objective by id if exists. Returns null otherwise.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Returns list of objectives, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID);

        /// <summary>
        /// Generates report about selected objectives.
        /// </summary>
        /// <param name="objectives">List of objective id's.</param>
        /// <param name="path">Path to report storage.</param>
        /// <param name="userID">ID of the user, who generates the report.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>Object representing the result of report creation process.</returns>
        Task<ObjectiveReportCreationResultDto> GenerateReport(IEnumerable<ID<ObjectiveDto>> objectives, string path, int userID, string projectName);

        ///// <summary>
        ///// Gets list of dynamic fields owned by objective.
        ///// </summary>
        ///// <param name="objectiveID">Objective's ID.</param>
        ///// <returns></returns>
        //Task<ICollection<IDynamicFieldDto>> GetRequiredDynamicFields(ID<ObjectiveDto> objectiveID);
    }
}
