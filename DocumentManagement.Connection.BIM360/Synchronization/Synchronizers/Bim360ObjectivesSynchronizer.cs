﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json.Linq;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    public class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly IssuesService issuesService;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly FoldersSyncHelper folderSyncHelper;
        private readonly ProjectsHelper projectsHelper;
        private readonly HubsHelper hubsHelper;

        public Bim360ObjectivesSynchronizer(Bim360ConnectionContext context)
        {
            issuesService = context.IssuesService;
            itemsSyncHelper = new ItemsSyncHelper(context.ItemsService, context.ProjectsService, context.ObjectsService);
            folderSyncHelper = new FoldersSyncHelper(context.FoldersService, context.ProjectsService);
            projectsHelper = new ProjectsHelper(context.ProjectsService);
            hubsHelper = new HubsHelper(context.HubsService);
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();

            // TODO: check will issues containerId recorded as ProjectExternalID or it is necessary to get it in another way (retrieve from projectservice by id)
            var hub = await hubsHelper.GetDefaultHubAsync();
            var project = await projectsHelper.GetProjectAsync(hub.ID, p => p.ID == obj.ProjectExternalID);
            var containerId = project.Relationships.IssuesContainer.Data.ID;
            var created = await issuesService.PostIssueAsync(containerId, issue);
            var added = await AddItems(obj.Items, hub.ID, project.ID, created.ID, containerId);
            if (!added.Any())
                return null;

            return created.ToExternalDto();
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();
            var containerId = GetContainerId(issue);
            issue = await issuesService.GetIssueAsync(containerId, issue.ID);
            issue.Attributes.Status = ISSUE_STATUS_CLOSED;
            var updatedIssue = await issuesService.PatchIssueAsync(containerId, issue);

            return updatedIssue.ToExternalDto();
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();
            var containerId = GetContainerId(issue);
            var updatedIssue = await issuesService.PatchIssueAsync(containerId, issue);

            return updatedIssue.ToExternalDto();
        }

        private async Task<IEnumerable<Item>> AddItems(
            ICollection<ItemExternalDto> items,
            string hubId,
            string projectId,
            string issueId,
            string containerId)
        {
            var resultItems = new List<Item>();
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.ExternalID))
                {
                    var folder = await folderSyncHelper.GetDefaultFolderAsync(hubId, projectId);
                    if (folder == default)
                        return null;

                    var posted = await itemsSyncHelper.PostItem(item, folder, projectId);
                    var attached = await AttachItem(posted, issueId, containerId);
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

        private string GetContainerId(Issue issue)
        {
            if (issue?.Relationships?.Container == null)
                return default;

            var container = JToken.Parse(issue.Relationships.Container);
            if (container == null)
                return default;

            var selfLink = (string)container["links"]["self"];
            var urlParts = selfLink.Split('/');
            var containersWordIndex = Array.FindIndex(urlParts, p => p == "containers");
            var containerId = urlParts[containersWordIndex + 1];

            return containerId;
        }
    }
}
