using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Finders
{
    internal class ItemAttacher : IAttacher<Item>
    {
        private readonly ILogger<ItemAttacher> logger;
        private readonly DMContext context;

        public ItemAttacher(ILogger<ItemAttacher> logger, DMContext context)
        {
            this.logger = logger;
            this.context = context;

            logger.LogTrace("ItemFinder created");
        }

        public async Task AttachExisting(SynchronizingTuple<Item> tuple)
        {
            logger.LogTrace("AttachExisting started with tuple {@Object}", tuple);
            if (tuple.Local != null && tuple.Synchronized != null)
                return;

            var externalId = tuple.ExternalID;
            var path = (string)tuple.GetPropertyValue(nameof(Item.RelativePath));

            Project localProject;
            Project syncProject;

            if (tuple.Local == null)
            {
                if (tuple.Synchronized == null)
                    (localProject, syncProject) = await GetProjectsByRemote(GetProjectId(tuple.Remote));
                else
                    (localProject, syncProject) = await GetProjectsBySynchronized(GetProjectId(tuple.Synchronized));

                tuple.Local = await GetItem(localProject, syncProject, externalId, path, false);
                logger.LogDebug("Local item: {@Object}", tuple.Local?.ID);
            }
            else
            {
                (localProject, syncProject) = await GetProjectsByLocal(GetProjectId(tuple.Local));
            }

            logger.LogDebug("Local project {@Local}, synchronized {@Synchronized}", localProject?.ID, syncProject?.ID);

            if (tuple.Synchronized == null && tuple.Local != null)
            {
                var localItem = tuple.Local;
                tuple.Synchronized = localItem.SynchronizationMateID == null
                    ? await GetItem(localProject, syncProject, externalId, path, true)
                    : await context.Items.FindAsync(localItem.SynchronizationMateID);
            }

            logger.LogDebug("Synchronized item: {@Object}", tuple.Synchronized?.ID);
        }

        private int GetProjectId(Item item)
        {
            logger.LogTrace("GetProjectId started with item {@Object}", item);
            var projectID = item.ProjectID ?? item.Objectives?.FirstOrDefault()?.Objective.ProjectID;
            logger.LogDebug("Project of item {@Item} - {@Project}", item.RelativePath, projectID);
            if (projectID == null)
                throw new ArgumentException("Item does not contain project");
            return projectID.Value;
        }

        private async Task<Item> GetItem(Project localProject, Project syncProject, string externalId, string path, bool isSynchronized)
        {
            logger.LogTrace(
                "GetItem started for {@Path}({@Id}), synchronized: {IsSynchronized}",
                path,
                externalId,
                isSynchronized);
            Expression<Func<Item, bool>> predicate;
            if (!isSynchronized)
            {
                int localProjectID = localProject?.ID ?? -1;
                int syncProjectID = syncProject?.ID ?? -1;
                predicate = i => i.ProjectID == localProjectID ||
                    i.Objectives.Any(o => o.Objective.ProjectID == localProjectID) ||
                    i.SynchronizationMate.ProjectID == syncProjectID ||
                    i.SynchronizationMate.Objectives.Any(o => o.Objective.ProjectID == syncProjectID);
            }
            else
            {
                predicate = i => i.ProjectID == syncProject.ID ||
                    i.Objectives.Any(o => o.Objective.ProjectID == syncProject.ID);
            }

            return await context.Items
               .Where(predicate)
               .FirstOrDefaultAsync(i => i.ExternalID == externalId || i.RelativePath == path);
        }

        private async Task<(Project localProject, Project syncProject)> GetProjectsByRemote(int? remoteProjectId)
        {
            logger.LogTrace("GetProjectsByRemote started for project {@Project}", remoteProjectId);
            var remoteProject = await context.Projects.AsNoTracking()
               .Include(x => x.SynchronizationMate)
               .FirstOrDefaultAsync(x => x.ID == remoteProjectId);
            Project localProject;
            Project syncProject;

            if (remoteProject.IsSynchronized)
            {
                localProject = await context.Projects.AsNoTracking()
                   .FirstOrDefaultAsync(x => x.SynchronizationMateID == remoteProject.ID);
                syncProject = remoteProject;
            }
            else
            {
                localProject = remoteProject;
                syncProject = localProject.SynchronizationMate;
            }

            return (localProject, syncProject);
        }

        private async Task<(Project localProject, Project syncProject)> GetProjectsBySynchronized(int syncProjectId)
        {
            logger.LogTrace("GetProjectsBySynchronized started for project {@Project}", syncProjectId);
            var localProject = await context.Projects.AsNoTracking()
               .FirstOrDefaultAsync(x => x.SynchronizationMateID == syncProjectId);
            var syncProject = await context.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.ID == syncProjectId);

            return (localProject, syncProject);
        }

        private async Task<(Project localProject, Project syncProject)> GetProjectsByLocal(int localProjectId)
        {
            logger.LogTrace("GetProjectsByLocal started for project {@Project}", localProjectId);
            var localProject = await context.Projects.AsNoTracking()
               .Include(x => x.SynchronizationMate)
               .FirstOrDefaultAsync(x => x.ID == localProjectId);
            var syncProject = localProject.SynchronizationMate;

            return (localProject, syncProject);
        }
    }
}
