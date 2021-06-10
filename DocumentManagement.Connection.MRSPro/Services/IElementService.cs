using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    internal interface IElementService
    {
        Task<IEnumerable<IElement>> GetAll(DateTime date);

        Task<IElement> TryGetById(string id);

        Task<IEnumerable<IElement>> TryGetByIds(IReadOnlyCollection<string> ids);
    }
}