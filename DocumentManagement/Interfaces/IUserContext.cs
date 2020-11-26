using DocumentManagement.Interface.Models;

namespace DocumentManagement.Services
{
    internal interface IUserContext
    {
        User CurrentUser { get; }
        bool IsInRole(string role);
    }
}
