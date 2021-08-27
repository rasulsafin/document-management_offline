using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Pagination
{
    public class MetaStrategy : ACountStrategy<Meta>
    {
        protected override string PropertyName => Constants.META_PROPERTY;

        protected override int GetCount(Meta page)
            => page.RecordCount ?? 0;
    }
}
