using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Interface.Services
{
    public interface IAuthorizationService
    {
        Task<IEnumerable<string>> GetAllRoles();
        Task AddRole(ID<User> userID, string role);
        Task RemoveRole(ID<User> userID, string role);
        Task<IEnumerable<string>> GetUserRoles(ID<User> userID);
        Task<bool> IsInRole(ID<User> userID, string role);
    }
}
