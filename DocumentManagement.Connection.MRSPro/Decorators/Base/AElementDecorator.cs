using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public abstract class AElementDecorator : IElementDecorator, IElementConvertible
    {
        public abstract Task<ObjectiveExternalDto> ConvertToDto(IElement element);

        public abstract Task<IElement> ConvertToModel(ObjectiveExternalDto element);

        public abstract Task<IEnumerable<IElement>> GetAll(DateTime date);

        public abstract Task<bool> DeleteElementById(string id);

        public abstract Task<IElement> GetElementById(string id);

        public abstract Task<IEnumerable<IElement>> GetElementsByIds(IReadOnlyCollection<string> ids);

        public abstract Task<IElement> PatchElement(UpdatedValues valuesToPatch);

        public abstract Task<IElement> PostElement(IElement element);
    }
}
