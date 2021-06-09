using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    internal class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly IssuesService issuesService;
        private readonly ItemsService itemsService;
        private readonly ConverterAsync<ObjectiveExternalDto, Issue> convertToIssueAsync;
        private readonly ConverterAsync<IssueSnapshot, ObjectiveExternalDto> convertToDtoAsync;
        private readonly IBim360SnapshotFiller filler;
        private readonly FoldersService foldersService;
        private readonly Authenticator authenticator;
        private readonly Bim360ConnectionContext context;

        public Bim360ObjectivesSynchronizer(
            Bim360ConnectionContext context,
            ItemsSyncHelper itemsSyncHelper,
            IssuesService issuesService,
            ItemsService itemsService,
            ConverterAsync<ObjectiveExternalDto, Issue> convertToIssueAsync,
            ConverterAsync<IssueSnapshot, ObjectiveExternalDto> convertToDtoAsync,
            IBim360SnapshotFiller filler,
            FoldersService foldersService,
            Authenticator authenticator)
        {
            this.context = context;
            this.itemsSyncHelper = itemsSyncHelper;
            this.issuesService = issuesService;
            this.itemsService = itemsService;
            this.convertToIssueAsync = convertToIssueAsync;
            this.convertToDtoAsync = convertToDtoAsync;
            this.filler = filler;
            this.foldersService = foldersService;
            this.authenticator = authenticator;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var issue = await convertToIssueAsync(obj);
            var project = GetProjectSnapshot(obj);

            var snapshot = new IssueSnapshot(issue, project);
            await LinkTarget(obj, project, issue);
            await SetGlobalOffset(snapshot);
            var created = await issuesService.PostIssueAsync(project.IssueContainer, issue);
            snapshot.Entity = created;
            project.Issues.Add(created.ID, snapshot);
            var parsedToDto = await convertToDtoAsync(snapshot);

            if (obj.Items?.Any() ?? false)
            {
                snapshot.Items = new List<ItemSnapshot>();
                var added = await AddItems(obj.Items, project.ID, created, project.IssueContainer);
                snapshot.Items.AddRange(added.Select(x => new ItemSnapshot(x)));
                parsedToDto.Items = snapshot.Items.Select(i => i.Entity.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var issue = await convertToIssueAsync(obj);
            var project = GetProjectSnapshot(obj);
            var snapshot = project.Issues[obj.ExternalID];

            if (!issue.Attributes.PermittedStatuses.Contains(Status.Void))
            {
                issue.Attributes.Status = Status.Open;
                issue = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            }

            issue.Attributes.Status = Status.Void;
            snapshot.Entity = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            project.Issues.Remove(snapshot.ID);

            return await convertToDtoAsync(snapshot);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var issue = await convertToIssueAsync(obj);
            var project = GetProjectSnapshot(obj);
            var snapshot = project.Issues[obj.ExternalID];

            if (issue.Attributes.TargetUrn == null || issue.Attributes.StartingVersion == null)
            {
                await LinkTarget(obj, project, issue);
                await SetGlobalOffset(snapshot);
            }

            snapshot.Entity = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            var parsedToDto = await convertToDtoAsync(snapshot);

            if (obj.Items?.Any() ?? false)
            {
                snapshot.Items ??= new List<ItemSnapshot>();
                var added = await AddItems(obj.Items, project.ID, snapshot.Entity, project.ID);
                snapshot.Items.AddRange(added.Select(x => new ItemSnapshot(x)));
                parsedToDto.Items = snapshot.Items.Select(i => i.Entity.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            await filler.UpdateHubsIfNull();
            await filler.UpdateProjectsIfNull();
            await filler.UpdateIssuesIfNull(date);
            return context.Snapshot.IssueEnumerable.Where(x => x.Value.Entity.Attributes.UpdatedAt > date)
               .Select(x => x.Key)
               .ToList();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var result = new List<ObjectiveExternalDto>();

            foreach (var id in ids)
            {
                var found = context.Snapshot.IssueEnumerable.FirstOrDefault(x => x.Key == id).Value;

                if (found == null)
                {
                    foreach (var project in context.Snapshot.ProjectEnumerable)
                    {
                        var container = project.Value.IssueContainer;
                        Issue received = null;

                        try
                        {
                            received  = await issuesService.GetIssueAsync(container, id);
                        }
                        catch
                        {
                        }

                        if (received  != null)
                        {
                            if (IssueUtilities.IsRemoved(received))
                                break;

                            found = new IssueSnapshot(received, project.Value);
                            break;
                        }
                    }
                }

                if (found == null)
                    continue;

                found.Items = new List<ItemSnapshot>();

                var attachments = await issuesService.GetAttachmentsAsync(
                    found.ProjectSnapshot.IssueContainer,
                    found.ID);

                foreach (var attachment in attachments)
                {
                    var (item, version) = await itemsService.GetAsync(found.ProjectSnapshot.ID, attachment.Attributes.Urn);

                    found.Items.Add(new ItemSnapshot(item) { Version = version });
                }

                result.Add(await convertToDtoAsync(found));

                if (!found.ProjectSnapshot.Issues.ContainsKey(found.Entity.ID))
                    found.ProjectSnapshot.Issues.Add(found.Entity.ID, found);
            }

            return result;
        }

        private async Task<IEnumerable<Item>> AddItems(
            ICollection<ItemExternalDto> items,
            string projectId,
            Issue issue,
            string containerId)
        {
            var folder = context.Snapshot.ProjectEnumerable.First(x => x.Key == projectId).Value.ProjectFilesFolder;

            var resultItems = new List<Item>();
            var fileTuples = await foldersService.SearchAsync(projectId, folder.ID);
            var existingItems = fileTuples.Select(t => t.item);
            var attachment = await issuesService.GetAttachmentsAsync(containerId, issue.ID);

            foreach (var item in items)
            {
                // If item with the same name already exists add existing item
                var itemWithSameNameExists = existingItems.FirstOrDefault(i => i.Attributes.DisplayName.Equals(item.FileName, StringComparison.InvariantCultureIgnoreCase));
                if (itemWithSameNameExists != null)
                {
                    if (attachment.Any(x => x.Attributes.Name == item.FileName))
                    {
                        resultItems.Add(itemWithSameNameExists);
                        continue;
                    }

                    var attached = await AttachItem(itemWithSameNameExists, issue.ID, containerId);
                    if (attached)
                        resultItems.Add(itemWithSameNameExists);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.ExternalID))
                {
                    var posted = await itemsSyncHelper.PostItem(item, folder, projectId);
                    await Task.Delay(5000);
                    var attached = await AttachItem(posted, issue.ID, containerId);
                    if (attached)
                        resultItems.Add(posted);
                }
            }

            return resultItems;
        }

        private async Task<bool> AttachItem(Item posted, string issueId, string containerId)
        {
            var attachment = new Attachment
            {
                Attributes = new Attachment.AttachmentAttributes
                {
                    Name = posted.Attributes.DisplayName,
                    IssueId = issueId,
                    Urn = posted.ID,
                },
            };

            try
            {
                await issuesService.PostIssuesAttachmentsAsync(containerId, attachment);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private ProjectSnapshot GetProjectSnapshot(ObjectiveExternalDto obj)
            => context.Snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID)
               .Value;

        private async Task SetGlobalOffset(IssueSnapshot snapshot)
        {
            if (snapshot.Entity.Attributes.PushpinAttributes != null &&
                (snapshot.Entity.Attributes.PushpinAttributes.ViewerState.GlobalOffset ?? Vector3.Zero) ==
                Vector3.Zero && snapshot.Entity.Attributes.TargetUrn != null)
            {
                var filter = new Filter(
                    typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.TargetUrn)),
                    snapshot.Entity.Attributes.TargetUrn);
                var other = await issuesService.GetIssuesAsync(
                    snapshot.ProjectSnapshot.IssueContainer,
                    new[] { filter });
                var withGlobalOffset = other.FirstOrDefault(
                    x => (x.Attributes.PushpinAttributes?.ViewerState?.GlobalOffset ?? Vector3.Zero) != Vector3.Zero);
                var offset = withGlobalOffset?.Attributes.PushpinAttributes.ViewerState.GlobalOffset ?? Vector3.Zero;
                snapshot.Entity.Attributes.PushpinAttributes.ViewerState.GlobalOffset = offset;
                snapshot.Entity.Attributes.PushpinAttributes.Location -= offset;
            }
        }

        private async Task LinkTarget(ObjectiveExternalDto obj, ProjectSnapshot project, Issue issue)
        {
            if (obj.Location != null)
            {
                Item item;
                Version version;

                if (obj.Location.Item.ExternalID == null)
                {
                    var filter = new Filter(
                        typeof(Version.VersionAttributes).GetDataMemberName(
                            nameof(Version.VersionAttributes.DisplayName)),
                        obj.Location.Item.FileName);
                    var items = (await foldersService.SearchAsync(
                        project.ID,
                        project.ProjectFilesFolder.ID,
                        new[] { filter })).OrderByDescending(x => x.version?.Attributes.VersionNumber ?? 0);
                    (version, item) = items.FirstOrDefault(
                        x => x.version?.Attributes.StorageSize == new FileInfo(obj.Location.Item.FullPath).Length);
                    if (item == default && version == default)
                        (version, item) = items.FirstOrDefault();

                    if (item == default && version == default)
                    {
                        item = await itemsSyncHelper.PostItem(obj.Location.Item, project.ProjectFilesFolder, project.ID);
                        await Task.Delay(5000);
                        version = (await itemsService.GetAsync(project.ID, item.ID)).version;
                    }
                }
                else
                {
                    (item, version) = await itemsService.GetAsync(project.ID, obj.Location.Item.ExternalID);
                }

                issue.Attributes.TargetUrn = item?.ID;
                issue.Attributes.StartingVersion = version?.Attributes.VersionNumber;
            }
        }
    }
}
