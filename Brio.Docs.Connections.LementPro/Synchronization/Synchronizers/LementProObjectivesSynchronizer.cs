using Brio.Docs.Connections.LementPro.Models;
using Brio.Docs.Connections.LementPro.Services;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Connections.LementPro.Synchronization
{
    // TODO: use capture from context.
    public class LementProObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly TasksService tasksService;
        private readonly BimsService bimsService;
        private readonly ILogger<LementProObjectivesSynchronizer> logger;

        private List<ObjectiveExternalDto> objectives;

        public LementProObjectivesSynchronizer(
            LementProConnectionContext context,
            TasksService tasksService,
            BimsService bimsService,
            ILogger<LementProObjectivesSynchronizer> logger)
        {
            this.tasksService = tasksService;
            this.bimsService = bimsService;
            this.logger = logger;
            logger.LogTrace("LementProObjectivesSynchronizer created");
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            logger.LogTrace("Add started with obj: {@Objective}", obj);
            var lementIssue = obj.ToModelToCreate();
            var isItemsAdding = obj.Items?.Any() ?? false;

            if (isItemsAdding)
            {
                var fileIds = await ItemsSynchronizationHelper.UploadFilesAsync(obj.Items, tasksService.CommonRequests);
                lementIssue.FileIds = fileIds;
            }

            var createResult = await tasksService.CreateTaskAsync(lementIssue);
            logger.LogDebug("Created task: {@Objective}", createResult);
            if (!createResult.IsSuccess.GetValueOrDefault())
                return null;

            // Wait for creating
            await Task.Delay(3000);

            var task = await tasksService.GetTaskAsync(createResult.ID.Value);
            logger.LogDebug("Received task: {@Objective}", task);
            var parsedToDto = task.ToObjectiveExternalDto();
            if (isItemsAdding)
                parsedToDto.Items = await FindAttachedItems(task, obj.Items);

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            logger.LogTrace("Add started with obj: {@Objective}", obj);
            if (!int.TryParse(obj.ExternalID, out var taskId))
                return null;

            var deleted = await tasksService.DeleteTaskAsync(taskId);
            logger.LogDebug("Deleted task: {@Objective}", deleted);
            return deleted.ToObjectiveExternalDto();
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            logger.LogTrace("Update started with obj: {@Objective}", obj);
            var isItemsAdding = obj.Items?.Any() ?? false;
            if (!int.TryParse(obj.ExternalID, out var parsedId))
                return null;

            var existingObjectiveModel = await tasksService.GetTaskAsync(parsedId);
            logger.LogDebug("Received task: {@Objective}", existingObjectiveModel);
            var modelToUpdate = obj.ToModelToUpdate();

            if (isItemsAdding)
            {
                modelToUpdate = await ItemsSynchronizationHelper
                    .UpdateFilesAsync(existingObjectiveModel, modelToUpdate, obj.Items, tasksService.CommonRequests);
            }

            var updatedResult = await tasksService.UpdateTaskAsync(modelToUpdate);
            logger.LogDebug("Updated task: {@Objective}", updatedResult);
            var parsedResult = updatedResult.ToObjectiveExternalDto();
            logger.LogDebug("Mapped task: {@Objective}", parsedResult);

            if (isItemsAdding)
                parsedResult.Items = updatedResult.Values.Files.ToDtoItems(obj.Items);

            return parsedResult;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            logger.LogTrace("GetUpdatedIDs started with date: {Date}", date);
            await CheckCashedElements();
            return objectives
                .Where(o => o.UpdatedAt >= date)
                .Select(o => o.ExternalID).ToList();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            logger.LogTrace("Get started with ids: {@IDs}", ids);
            await CheckCashedElements();
            return objectives.Where(o => ids.Contains(o.ExternalID)).ToList();
        }

        private async Task<ICollection<ItemExternalDto>> FindAttachedItems(ObjectBase task, ICollection<ItemExternalDto> items)
        {
            logger.LogTrace("FindAttachedItems started with task: {@Objectives}, items: {@Items}", task, items);
            var resultItems = new List<ItemExternalDto>();
            if (task.Values?.BimRef?.ID != null)
            {
                var bim = await bimsService.GetBimLastVersion(task.Values.BimRef.ID.Value.ToString());
                logger.LogDebug("Received bim: {@Item}", bim);
                if (bim != null)
                    resultItems.Add(bim.ToItemExternalDto());
            }

            if (task.Values.Files != null)
            {
                var attachedFiles = task.Values.Files.ToDtoItems(items);
                logger.LogDebug("Mapped attached files: {@Items}", attachedFiles);
                resultItems.AddRange(attachedFiles);
            }

            return resultItems;
        }

        private async Task CheckCashedElements()
        {
            logger.LogTrace("CheckCashedElements started");

            if (objectives == null)
            {
                objectives = new List<ObjectiveExternalDto>();

                var lementTasks = await tasksService.GetAllTasksAsync();
                logger.LogDebug("Received tasks: {@Objectives}", lementTasks);
                var files = await bimsService.GetAllBimFilesAsync();
                logger.LogDebug("Received files: {@Items}", files);

                foreach (var task in lementTasks)
                {
                    // It is necessary to get full info about issue to get last updated info
                    var fullInfoTask = await tasksService.GetTaskAsync(task.ID.Value);
                    logger.LogDebug("Received task: {@Objective}", fullInfoTask);
                    var objective = fullInfoTask.ToObjectiveExternalDto();
                    objective.Items = GetIssueFiles(fullInfoTask, files);
                    logger.LogDebug("Mapped objective: {@Objective}", objectives);
                    objectives.Add(objective);
                }
            }
        }

        private ICollection<ItemExternalDto> GetIssueFiles(ObjectBase issue, IEnumerable<ObjectBase> allBims)
        {
            logger.LogTrace("GetIssueFiles started with issue: {@Objective}, allBims: {@Items}", issue, allBims);
            var files = new List<ItemExternalDto>();
            var bimRef = issue.Values.BimRef;
            var bimFile = allBims.FirstOrDefault(b => b.ID == bimRef?.ID)?.ToItemExternalDto();
            logger.LogDebug("Found bimFile: {@Item}", bimFile);
            if (bimFile != null)
                files.Add(bimFile);

            if (issue.Values.Files != null)
                files.AddRange(issue.Values.Files.ToDtoItems());
            logger.LogDebug("Found files: {@Items}", files);

            return files;
        }
    }
}
