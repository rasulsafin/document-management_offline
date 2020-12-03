using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IItemService
    {
        Task<ID<ItemDto>> Add(ItemToCreateDto data, ID<ProjectDto> parentProject);
        Task<ID<ItemDto>> Add(ItemToCreateDto data, ID<ObjectiveDto> parentObjective);

        Task<bool> Link(ID<ItemDto> itemID, ID<ProjectDto> projectID);
        Task<bool> Link(ID<ItemDto> itemID, ID<ObjectiveDto> objectiveID);

        Task<bool> Unlink(ID<ItemDto> itemID, ID<ProjectDto> projectID);
        Task<bool> Unlink(ID<ItemDto> itemID, ID<ObjectiveDto> objectiveID);

        Task<bool> Update(ItemDto item);
        Task<ItemDto> Find(ID<ItemDto> itemID);

        Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID);
        Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID);
    }
}
