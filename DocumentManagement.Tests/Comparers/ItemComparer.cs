using MRS.DocumentManagement.Database.Models;
using System.Diagnostics.CodeAnalysis;

namespace MRS.DocumentManagement.Tests
{
    internal class ItemComparer : AbstractModelComparer<Item>
    {
        public ItemComparer(bool ignoreIDs = false) : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] Item x, [DisallowNull] Item y)
        {
            var dataEquals = x.ItemType == y.ItemType && x.Name == y.Name;
            if (!IgnoreIDs)
                dataEquals = dataEquals && x.ID == y.ID;
            return dataEquals;
        }
    }
}
