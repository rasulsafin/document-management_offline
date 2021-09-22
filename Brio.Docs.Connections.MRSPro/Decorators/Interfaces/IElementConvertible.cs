using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Connections.MrsPro.Interfaces
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
