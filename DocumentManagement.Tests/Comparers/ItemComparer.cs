using MRS.DocumentManagement.Interface.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace MRS.DocumentManagement.Tests
{
    internal class ItemComparer : AbstractModelComparer<ItemDto>
    {
        public ItemComparer(bool ignoreIDs = false) : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] ItemDto x, [DisallowNull] ItemDto y)
        {
            var dataEquals = x.ItemType == y.ItemType && x.Path == y.Path;
            if (!IgnoreIDs)
                dataEquals = dataEquals && x.ID == y.ID;
            return dataEquals;
        }
    }
}
