using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Interfaces
{
    /// <summary>
    /// Element that can be converted to ObjectiveExternalDto.
    /// </summary>
    internal interface IElementConvertible
    {
        /// <summary>
        /// Converts element from MRSPro to objective DM.
        /// </summary>
        /// <param name="element">Element to convert.</param>
        /// <returns>Converted objective.</returns>
        Task<ObjectiveExternalDto> ConvertToDto(IElementObject element);

        /// <summary>
        /// Converts objective DM to proper element from MRSPro.
        /// </summary>
        /// <param name="element">Objective to convert.</param>
        /// <returns>Converted element.</returns>
        Task<IElementObject> ConvertToModel(ObjectiveExternalDto element);
    }
}
