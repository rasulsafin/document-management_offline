using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    public class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly IssuesService issuesService;
        private readonly Bim360ConnectionContext context;

        public Bim360ObjectivesSynchronizer(ForgeConnection forgeConnection, Bim360ConnectionContext context)
        {
            issuesService = new IssuesService(forgeConnection);
            this.context = context;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();

            // TODO: check will containerId recorded as ProjectExternalID or it is necessary to get it in another way
            var containerId = obj.ProjectExternalID;
            var created = await issuesService.PostIssueAsync(containerId, issue);
            await AddItems(obj.Items, created.ID, containerId);

            return created.ToExternalDto();
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<Item>> AddItems(ICollection<ItemExternalDto> items, string issueId, string containerId)
        {
            var resultItems = new List<Item>();
            Folder folder;
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.ExternalID))
                {
                    var topFolder = (await projectsService.GetTopFoldersAsync(hub.ID, project.ID)).LastOrDefault();
                    var posted = await itemsSynchronizer.PostItem(item, folder);
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
    }
}
