using System;
using System.Threading.Tasks;
using DocumentManagement.Database;
using DocumentManagement.Interface.Models;

namespace DocumentManagement.Services
{
    internal class SynchronizedUserService : UserService
    {
        public User CurrentUser { get; private set; }
        public event EventHandler<User> CurrentUserChanged;

        public SynchronizedUserService(DMContext context, User user) 
            : base(context)
        {
            CurrentUser = user;
        }

        public override async Task<bool> Delete(ID<User> userID)
        {
            var isDeleted = await base.Delete(userID);
            if (isDeleted && userID.IsValid && userID == CurrentUser.ID)
            {
                CurrentUser = User.Anonymous;
                CurrentUserChanged?.Invoke(this, CurrentUser);
            }
            return isDeleted;
        }

        public override async Task Update(User user)
        {
            await base.Update(user);
            if (user.ID.IsValid && user.ID == CurrentUser.ID)
            {
                CurrentUser = user;
                CurrentUserChanged?.Invoke(this, CurrentUser);
            }
        }
    }
}
