using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class IssuesService : Service, IElementService
    {
        private static readonly string BASE_URL = "/task";

        public IssuesService(MrsProHttpConnection connection)
            : base(connection) { }

        public async Task<IEnumerable<IElement>> GetAll(DateTime date)
        {
            // TODO: try request with data as query?
            var listOfAllObjectives = await HttpConnection.GetListOf<Issue>(BASE_URL);

            // TODO: Remove it;
            date = new DateTime(2021, 6, 6);

            var unixDate = date.ToUnixTime();
            var list = listOfAllObjectives.Where(o => o.LastModifiedDate > unixDate).ToArray();
            return list;
        }

        public async Task<IEnumerable<IElement>> TryGetByIds(IReadOnlyCollection<string> ids)
        {
            try
            {
                var idsStr = GetListAsString(ids);
                return await HttpConnection.GetListOf<Issue>(GetByIds(BASE_URL), idsStr);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IElement> TryGetById(string id)
        {
            try
            {
                var res = await HttpConnection.GetListOf<Issue>(GetByIds(BASE_URL), new[] { id });
                return res.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IElement> TryPost(IElement element)
        {
            var result =  await HttpConnection.PostJson<Issue>(BASE_URL, element as Issue);
            return result;
        }

        public async Task<IElement> TryPatch(UpdatedValues valuesToPatch)
        {
            var result = await HttpConnection.PatchJson<IEnumerable<Issue>, UpdatedValues>(BASE_URL, valuesToPatch);
            return result.FirstOrDefault();
        }
    }
}
