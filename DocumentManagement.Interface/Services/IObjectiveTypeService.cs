using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service for managing Objective Types.
    /// </summary>
    public interface IObjectiveTypeService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<ObjectiveTypeDto>> GetObjectiveTypes(ID<ConnectionTypeDto> id);

        /// <summary>
        ///
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ID<ObjectiveTypeDto>> Add(string typeName);

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Remove(ID<ObjectiveTypeDto> id);

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id);

        /// <summary>
        ///
        /// </summary>
        /// <param name="typename"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ObjectiveTypeDto> Find(string typename);
    }
}
