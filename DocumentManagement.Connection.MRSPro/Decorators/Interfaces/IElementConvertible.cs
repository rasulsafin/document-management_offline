using MRS.DocumentManagement.Interface.Dtos;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Interfaces
{
    internal interface IElementConvertible
    {
        Task<ObjectiveExternalDto> ConvertToDto(IElementObject element);

        Task<IElementObject> ConvertToModel(ObjectiveExternalDto element);
    }
}