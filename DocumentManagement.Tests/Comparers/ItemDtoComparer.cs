using System.Diagnostics.CodeAnalysis;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Tests
{
    internal class ItemDtoComparer : AbstractModelComparer<ItemDto>
    {
        public ItemDtoComparer(bool ignoreIDs = false) : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] ItemDto x, [DisallowNull] ItemDto y)
        {
            var dataEquals = x.ItemType == y.ItemType && x.RelativePath == y.RelativePath;
            if (!IgnoreIDs)
                dataEquals = dataEquals && x.ID == y.ID;
            return dataEquals;
        }
    }
}
