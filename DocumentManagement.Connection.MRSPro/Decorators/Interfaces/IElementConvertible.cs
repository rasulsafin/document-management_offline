using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    internal interface IElementConvertible
    {
        Task<ObjectiveExternalDto> ConvertToDto(IElement element);

        Task<IElement> ConvertToModel(ObjectiveExternalDto element);
    }
}