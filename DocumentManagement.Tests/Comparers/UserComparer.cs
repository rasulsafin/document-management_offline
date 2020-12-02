using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Tests
{
    internal class UserComparer : AbstractModelComparer<User>
    {
        public UserComparer(bool ignoreIDs) : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals(User x, User y)
        {
            var dataEqual = x.Login == y.Login && x.Name == y.Name;
            if (IgnoreIDs)
                return dataEqual;

            return dataEqual && x.ID == y.ID;
        }
    }
}
