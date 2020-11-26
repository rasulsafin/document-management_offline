using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Services
{
    internal interface IUserContext
    {
        User CurrentUser { get; }
        bool IsInRole(string role);
    }
}
