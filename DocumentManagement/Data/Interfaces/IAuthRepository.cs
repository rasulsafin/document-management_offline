using DocumentManagement.Models.Database;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public interface IAuthRepository
    {
        Task<UserDb> Login(string login, string password);
        Task<UserDb> Register(UserDb user, string password);
        Task<UserDb> Get(string login);
        Task<UserDb> Update(UserDb project);
        Task<bool> Delete(string login);
        Task<bool> IsExists(string login);
    }
}