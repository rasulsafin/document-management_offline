using System.Threading.Tasks;
using DocumentManagement.Interface.Models;

namespace DocumentManagement.Interface
{
    public interface IDocumentManagementApi
    {
        Task<IAuthenticatedAccess> Login(string login, string password);
        Task<IAuthenticatedAccess> Register(NewUser data);
    }
}
