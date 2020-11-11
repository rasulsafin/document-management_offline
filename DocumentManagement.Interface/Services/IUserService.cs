using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;

namespace DocumentManagement.Interface.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        Task<ID<User>> Add(NewUser data);
        Task Delete(ID<User> userID);
        Task Update(User user);
        Task<bool> VerifyPassword(ID<User> userID, string password);
        Task UpdatePassword(ID<User> userID, string newPass);
        Task<User> Find(ID<User> userID);
        Task<User> Find(string login);
        Task<bool> Exists(ID<User> userID);
        Task<bool> Exists(string login);
        Task<User> GetCurrentUser();
    }
}
