using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    /// <summary>
    ///
    /// </summary>
    internal interface IDFHelper<T, TID>
    {
        /// <summary>
        ///
        /// </summary>
        string ID { get; }

        /// <summary>
        ///
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        IOrderedEnumerable<TID> Order(IEnumerable<T> types);

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetDisplayName(T type);

        /// <summary>
        ///
        /// </summary>
        /// <param name="issuesService"></param>
        /// <param name="projectSnapshot"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetFromRemote(IssuesService issuesService, ProjectSnapshot projectSnapshot);
    }
}
