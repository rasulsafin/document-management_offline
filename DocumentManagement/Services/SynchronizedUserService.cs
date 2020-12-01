using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Services
{
    public class SynchronizedUserService : UserService
    {
        public UserDto CurrentUser { get; private set; }
        public event EventHandler<UserDto> CurrentUserChanged;

        public SynchronizedUserService(DMContext context, UserDto user) 
            : base(context)
        {
            CurrentUser = user;
        }

        public override async Task<bool> Delete(ID<UserDto> userID)
        {
            var isDeleted = await base.Delete(userID);
            if (isDeleted && userID.IsValid && userID == CurrentUser.ID)
            {
                CurrentUser = UserDto.Anonymous;
                CurrentUserChanged?.Invoke(this, CurrentUser);
            }
            return isDeleted;
        }

        public override async Task<bool> Update(UserDto user)
        {
            await base.Update(user);
            if (user.ID.IsValid && user.ID == CurrentUser.ID)
            {
                CurrentUser = user;
                CurrentUserChanged?.Invoke(this, CurrentUser);
            }
            return true;
        }
    }
}
