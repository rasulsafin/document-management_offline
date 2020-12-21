using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IItemService
    {
        Task<bool> Update(ItemDto item);
        Task<ItemDto> Find(ID<ItemDto> itemID);

        Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID);
        Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID);
    }
}
