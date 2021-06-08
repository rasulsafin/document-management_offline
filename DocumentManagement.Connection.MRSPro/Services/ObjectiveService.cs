﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Properties;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    /// <summary>
    /// Works with only MrsPro.Models
    /// </summary>
    public class ObjectiveService : Service
    {
        public ObjectiveService(MrsProHttpConnection connection)
            : base(connection) { }

        internal async Task<IEnumerable<Objective>> GetObjectives(DateTime date)
        {
            var listOfAllObjectives = await HttpConnection.SendAsync<IEnumerable<Objective>>(
                HttpMethod.Get,
                URLs.GetObjectives);

            date = new DateTime(2021, 6, 6);

            var unixDate = date.ToUnixTime();
            var list = listOfAllObjectives.Where(o => o.LastModifiedDate > unixDate).ToArray();
            return list;
        }

        internal async Task<IEnumerable<Objective>> GetObjectivesById(IReadOnlyCollection<string> ids)
        {
            return await GetById<IEnumerable<Objective>>(GetValueString(ids), URLs.GetObjectivesByIds);
        }

        internal async Task<Objective> TryGetObjectiveById(string id)
        {
            try
            {
                var res = await GetById<IEnumerable<Objective>>(id, URLs.GetObjectivesByIds);
                return res.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}
