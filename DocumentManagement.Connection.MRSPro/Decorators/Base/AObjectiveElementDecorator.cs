using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Interfaces;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public abstract class AObjectiveElementDecorator : IElementDecorator<IElementObject, IElementAttachment>, IElementConvertible<ObjectiveExternalDto, IElementObject>
    {
        public abstract Task<ObjectiveExternalDto> ConvertToDto(IElementObject element);

        public abstract Task<IElementObject> ConvertToModel(ObjectiveExternalDto element);

        public abstract Task<IEnumerable<IElementObject>> GetAll(DateTime date);

        public abstract Task<bool> DeleteElementById(string id);

        public abstract Task<IElementObject> GetElementById(string id);

        public abstract Task<IEnumerable<IElementObject>> GetElementsByIds(IReadOnlyCollection<string> ids);

        public abstract Task<IElementObject> PatchElement(UpdatedValues valuesToPatch);

        public abstract Task<IElementObject> PostElement(IElementObject element);

        public abstract Task<IEnumerable<IElementAttachment>> GetAttachments(string id);
    }
}
