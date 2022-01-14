using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Synchronization.Models;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.Synchronization.Interfaces
{
    /// <summary>
    /// Represents tool to link issue to a model.
    /// </summary>
    internal interface IIssueToModelLinker
    {
        /// <summary>
        /// Links the issue to the model. Creates pushpin attributes.
        /// </summary>
        /// <param name="issueToChange">The issue that must be a pushpin issue.</param>
        /// <param name="objective">The objective contains location info.</param>
        /// <param name="target">The target model.</param>
        /// <returns>The task of the operation. The result of the task is issue with needed info & redirected info about original model.</returns>
        Task<(Issue, LinkedInfo)> LinkToModel(Issue issueToChange, ObjectiveExternalDto objective, ItemSnapshot target);
    }
}
