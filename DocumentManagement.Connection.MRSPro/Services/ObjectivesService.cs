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
    public class ObjectivesService : Service
    {
        public ObjectivesService(MrsProHttpConnection connection)
            : base(connection) { }

        public async Task<IEnumerable<Objective>> GetObjectives(DateTime date)
        {
            // TODO: try request with data as query?
            var listOfAllObjectives = await HttpConnection.GetAll<Objective>(URLs.GetObjectives);

            // TODO: Remove it;
            date = new DateTime(2021, 6, 6);

            var unixDate = date.ToUnixTime();
            var list = listOfAllObjectives.Where(o => o.LastModifiedDate > unixDate).ToArray();
            return list;
        }

        public async Task<IEnumerable<Objective>> TryGetObjectivesById(IReadOnlyCollection<string> ids)
        {
            try
            {
                return await HttpConnection.GetByIds<Objective>(URLs.GetObjectivesByIds, ids);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Objective> TryGetObjectiveById(string id)
        {
            try
            {
                var res = await HttpConnection.GetByIds<Objective>(URLs.GetObjectivesByIds, id);
                return res.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}
