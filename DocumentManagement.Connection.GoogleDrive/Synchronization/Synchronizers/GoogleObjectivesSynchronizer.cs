using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.GoogleDrive.Synchronization
{
    public class GoogleObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        public Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
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
