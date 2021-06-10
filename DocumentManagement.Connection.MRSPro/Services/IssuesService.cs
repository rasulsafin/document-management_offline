using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Properties;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    /// <summary>
    /// Works with MrsPro.Models only.
    /// </summary>
    public class IssuesService : Service, IElementService
    {
        public IssuesService(MrsProHttpConnection connection)
            : base(connection) { }

        public async Task<IEnumerable<IElement>> GetAll(DateTime date)
        {
            // TODO: try request with data as query?
            var listOfAllObjectives = await HttpConnection.GetAll<Issue>(URLs.GetObjectives);

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
                return await HttpConnection.GetByIds<Issue>(URLs.GetObjectivesByIds, ids);
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
                var res = await HttpConnection.GetByIds<Issue>(URLs.GetObjectivesByIds, id);
                return res.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<Issue> PostObjective(Issue objective)
        {
            var result =  await HttpConnection.SendAsyncJson<Issue, Issue>(URLs.PostObjective, objective);
            return result;
        }
    }
}
