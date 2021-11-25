using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Interfaces;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class StatusEnumCreator : IEnumCreator<Status, StatusSnapshot, string>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.Status);

        private static readonly string DISPLAY_NAME = MrsConstants.STATUS_FIELD_NAME;

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public bool CanBeNull => false;

        public string NullID => throw new NotSupportedException();

        public IEnumerable<string> GetOrderedIDs(IEnumerable<StatusSnapshot> variants)
            => variants.OrderBy(x => x.Entity).Select(x => x.Entity.GetEnumMemberValue());

        public string GetVariantDisplayName(StatusSnapshot variant)
            => variant.Entity switch
            {
                Status.Undefined => throw new NotSupportedException(),
                Status.Void => throw new NotSupportedException(),
                Status.Draft => MrsConstants.DRAFT_STATUS_TITLE,
                Status.Open => MrsConstants.OPEN_STATUS_TITLE,
                Status.Closed => MrsConstants.CLOSED_STATUS_TITLE,
                Status.Answered => MrsConstants.ANSWERED_STATUS_TITLE,
                Status.WorkCompleted => MrsConstants.WORK_COMPLETED_STATUS_TITLE,
                Status.ReadyToInspect => MrsConstants.READY_TO_INSPECT_STATUS_TITLE,
                Status.NotApproved => MrsConstants.NOT_APPROVED_STATUS_TITLE,
                Status.InDispute => MrsConstants.IN_DISPUTE_STATUS_TITLE,
                _ => throw new ArgumentOutOfRangeException(nameof(variant.Entity), "Variant incorrect")
            };

        public Task<IEnumerable<StatusSnapshot>> GetVariantsFromRemote(ProjectSnapshot projectSnapshot)
            => Task.FromResult(
                Enum.GetValues<Status>()
                   .Where(x => x != Status.Undefined && x != Status.Void)
                   .Select(x => new StatusSnapshot(x, projectSnapshot)));

        public IEnumerable<StatusSnapshot> GetSnapshots(ProjectSnapshot project)
            => project.Statuses.Values;

        public IEnumerable<string> DeserializeID(string externalID)
            => DynamicFieldUtilities.DeserializeID<string>(externalID);
    }
}
