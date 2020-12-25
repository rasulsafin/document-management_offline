using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class ItemService : IItemService
    {
        public Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Update(ItemDto item)
        {
            throw new System.NotImplementedException();
        }
    }
}
