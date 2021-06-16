using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public interface IElementService
    {
        Task<IEnumerable<IElement>> GetAll(DateTime date);

        Task<IElement> TryGetById(string id);

        Task<IEnumerable<IElement>> TryGetByIds(IReadOnlyCollection<string> ids);
        Task<IElement> TryPatch(UpdatedValues valuesToPatch);
        Task<IElement> TryPost(IElement element);
    }
}
