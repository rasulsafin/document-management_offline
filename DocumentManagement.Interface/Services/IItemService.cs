using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;

namespace DocumentManagement.Interface.Services
{
    public interface IItemService
    {
        Task<ID<Item>> Add(ItemToCreate data, ID<Project> parentProject);
        Task<ID<Item>> Add(ItemToCreate data, ID<Objective> parentObjective);

        Task Link(ID<Item> itemID, ID<Project> projectID);
        Task Link(ID<Item> itemID, ID<Objective> objectiveID);

        Task Unlink(ID<Item> itemID, ID<Project> projectID);
        Task Unlink(ID<Item> itemID, ID<Objective> objectiveID);

        Task Update(Item item);
        Task<Item> Find(ID<Item> itemID);
        Task<Item> Find(string path);

        Task<IEnumerable<Item>> GetItems(ID<Project> projectID);
        Task<IEnumerable<Item>> GetItems(ID<Objective> objectiveID);
    }
}
