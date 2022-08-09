using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class ItemService : ServiceBase, IItemService
    {
        private static readonly string PATH = "Items";
        private static readonly string PROJECT = "project";
        private static readonly string OBJECTIVE = "objective";

        public ItemService(Connection connection)
            : base(connection)
        {
        }

        public async Task<bool> Update(ItemDto itemDto)
            => await Connection.PutObjectJsonAsync<ItemDto, bool>($"{PATH}", itemDto);

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
            => await Connection.GetDataAsync<ItemDto>($"{PATH}/{{0}}", itemID);

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> pProjectDtoID)
            => await Connection.GetDataAsync<IEnumerable<ItemDto>>($"{PATH}/{PROJECT}/{{0}}", pProjectDtoID);

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveDtoID)
            => await Connection.GetDataAsync<IEnumerable<ItemDto>>($"{PATH}/{OBJECTIVE}/{{0}}", objectiveDtoID);

        public async Task<RequestID> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
            => await Connection.PostObjectJsonAsync<IEnumerable<ID<ItemDto>>, RequestID>($"{PATH}/{{0}}", itemIds, userID);

        public async Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds)
            => await Connection.PostObjectJsonAsync<IEnumerable<ID<ItemDto>>, bool>($"{PATH}/delete", itemIds);
    }
}
