using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface
{
    public interface IConnectionContext
    {
        Task<IReadOnlyCollection<ProjectExternalDto>> Projects { get; }

        Task<IReadOnlyCollection<ObjectiveExternalDto>> Objectives { get; }

        ISynchronizer<ProjectExternalDto> ProjectsSynchronizer { get; }

        ISynchronizer<ObjectiveExternalDto> ObjectivesSynchronizer { get; }
    }
}
