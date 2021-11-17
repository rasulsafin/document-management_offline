using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities.Objective
{
    internal class ObjectiveGetter
    {
        private readonly SnapshotGetter snapshotGetter;
        private readonly IssuesService issuesService;
        private readonly IssueSnapshotUtilities snapshotUtilities;
        private readonly SnapshotUpdater snapshotUpdater;
        private readonly IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto;

        public ObjectiveGetter(
            SnapshotGetter snapshotGetter,
            IssuesService issuesService,
            IssueSnapshotUtilities snapshotUtilities,
            SnapshotUpdater snapshotUpdater,
            IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto)
        {
            this.snapshotGetter = snapshotGetter;
            this.issuesService = issuesService;
            this.snapshotUtilities = snapshotUtilities;
            this.snapshotUpdater = snapshotUpdater;
            this.converterToDto = converterToDto;
        }

        public IReadOnlyCollection<string> GetUpdatedIDs(DateTime date)
        {
            return snapshotGetter.GetIssues().Where(x => x.Entity.Attributes.UpdatedAt > date)
               .Select(x => x.ID)
               .ToList();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IEnumerable<string> ids)
        {
            var result = new List<ObjectiveExternalDto>();

            foreach (var id in ids)
            {
                var dto = await Get(id);

                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        private async Task<ObjectiveExternalDto> Get(string id)
        {
            var found = snapshotGetter.GetIssue(id) ?? await Find(id);
            return found == null ? null : await converterToDto.Convert(found);
        }

        private async Task<IssueSnapshot> Find(string id)
        {
            foreach (var project in snapshotGetter.GetProjects())
            {
                var container = project.IssueContainer;
                Issue received = null;

                try
                {
                    received = await issuesService.GetIssueAsync(container, id);
                }
                catch
                {
                }

                if (received != null)
                {
                    if (IssueUtilities.IsRemoved(received))
                        break;

                    var found = snapshotUpdater.CreateIssue(project, received);
                    found.Attachments = await snapshotUtilities.GetAttachments(found, project);
                    found.Comments = await snapshotUtilities.GetComments(found, project);
                    return found;
                }
            }

            return null;
        }
    }
}
