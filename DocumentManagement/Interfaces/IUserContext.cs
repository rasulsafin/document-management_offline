using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Services
{
    public interface IUserContext
    {
        User CurrentUser { get; }
        bool IsInRole(string role);
    }
}
