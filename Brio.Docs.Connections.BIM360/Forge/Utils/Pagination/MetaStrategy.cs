using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Forge.Utils.Pagination
{
    public class MetaStrategy : ACountStrategy<Meta>
    {
        protected override string PropertyName => Constants.META_PROPERTY;

        protected override int GetCount(Meta page)
            => page.RecordCount ?? 0;

        protected override string SetPageParameters(string command, int limit, int offset)
            => ForgeConnection.SetParameter(command, PageFilter.ByOffset(limit, offset));
    }
}
