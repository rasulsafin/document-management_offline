using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Interfaces
{
    /// <summary>
    /// Element that can be converted to dto.
    /// </summary>
    /// <typeparam name="TDto">Dto type.</typeparam>
    /// <typeparam name="TModel">Model type.</typeparam>
    internal interface IElementConvertible<TDto, TModel>
    {
        /// <summary>
        /// Converts element from MRSPro to objective DM.
        /// </summary>
        /// <param name="element">Element to convert.</param>
        /// <returns>Converted objective.</returns>
        Task<TDto> ConvertToDto(TModel element);

        /// <summary>
        /// Converts objective DM to proper element from MRSPro.
        /// </summary>
        /// <param name="element">Objective to convert.</param>
        /// <returns>Converted element.</returns>
        Task<TModel> ConvertToModel(TDto element);
    }
}
