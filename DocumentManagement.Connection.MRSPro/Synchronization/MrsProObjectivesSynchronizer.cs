using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        public Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }
    }
}
