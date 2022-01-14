using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities.Objective
{
    internal class ObjectiveRemover
    {
        private readonly SnapshotGetter snapshot;
        private readonly SnapshotUpdater snapshotUpdater;
        private readonly IIssuesService issuesService;
        private readonly IConverter<ObjectiveExternalDto, Issue> converterToIssue;
        private readonly IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto;

        public ObjectiveRemover(
            SnapshotGetter snapshot,
            SnapshotUpdater snapshotUpdater,
            IIssuesService issuesService,
            IConverter<ObjectiveExternalDto, Issue> converterToIssue,
            IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto)
        {
            this.snapshot = snapshot;
            this.snapshotUpdater = snapshotUpdater;
            this.issuesService = issuesService;
            this.converterToIssue = converterToIssue;
            this.converterToDto = converterToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            var project = snapshot.GetProject(obj.ProjectExternalID);
            var issue = await converterToIssue.Convert(obj);

            if (!issue.Attributes.PermittedStatuses.Contains(Status.Void))
            {
                issue.Attributes.Status = Status.Open;
                issue = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            }

            issue.Attributes.Status = Status.Void;
            issue = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            var issueSnapshot = snapshotUpdater.UpdateIssue(project, issue);
            var parsedToDto = await converterToDto.Convert(issueSnapshot);
            return parsedToDto;
        }
    }
}
