using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public abstract class AElementService : Service, IElementService, IElementConvertible
    {
        protected AElementService(MrsProHttpConnection connection)
            : base(connection)
        {
        }

        public abstract Task<ObjectiveExternalDto> ConvertToDto(IElement element);
        public abstract Task<IElement> ConvertToModel(ObjectiveExternalDto element);
        public abstract Task<IEnumerable<IElement>> GetAll(DateTime date);
        public abstract Task<bool> TryDelete(string id);
        public abstract Task<IElement> TryGetById(string id);
        public abstract Task<IEnumerable<IElement>> TryGetByIds(IReadOnlyCollection<string> ids);
        public abstract Task<IElement> TryPatch(UpdatedValues valuesToPatch);
        public abstract Task<IElement> TryPost(IElement element);
    }
}
