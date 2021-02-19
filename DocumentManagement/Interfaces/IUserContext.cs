using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Services
{
    public interface IUserContext
    {
        UserDto CurrentUser { get; }
        bool IsInRole(string role);
    }
}
