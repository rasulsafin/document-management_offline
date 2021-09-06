using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Pagination
{
    public class PaginationStrategy : ACountStrategy<Models.Bim360.Pagination>
    {
        protected override string PropertyName => Constants.PAGINATION_PROPERTY;

        protected override int GetCount(Models.Bim360.Pagination page)
            => page.TotalResults ?? 0;

        protected override string SetPageParameters(string command, int limit, int offset)
            => ForgeConnection.SetParameters(
                command,
                new IQueryParameter[]
                {
                    new Filter(Constants.LIMIT_PARAMETER_NAME, limit.ToString()),
                    new Filter(Constants.OFFSET_PARAMETER_NAME, offset.ToString()),
                });
    }
}
