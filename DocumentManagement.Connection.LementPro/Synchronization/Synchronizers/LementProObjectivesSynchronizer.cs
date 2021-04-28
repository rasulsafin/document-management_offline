using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
{
    // TODO: use capture from context.
    public class LementProObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly TasksService tasksService;
        private readonly BimsService bimsService;

        private List<ObjectiveExternalDto> objectives;

        public LementProObjectivesSynchronizer(
            LementProConnectionContext context,
            TasksService tasksService,
            BimsService bimsService)
        {
            this.tasksService = tasksService;
            this.bimsService = bimsService;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var lementIssue = obj.ToModelToCreate();
            var isItemsAdding = obj.Items?.Any() ?? false;

            if (isItemsAdding)
            {
                var fileIds = await ItemsSynchronizationHelper.UploadFilesAsync(obj.Items, tasksService.CommonRequests);
                lementIssue.FileIds = fileIds;
            }

            var createResult = await tasksService.CreateTaskAsync(lementIssue);
            if (!createResult.IsSuccess.GetValueOrDefault())
                return null;

            // Wait for creating
            await Task.Delay(3000);

            var task = await tasksService.GetTaskAsync(createResult.ID.Value);
            var parsedToDto = task.ToObjectiveExternalDto();
            if (isItemsAdding)
                parsedToDto.Items = await FindAttachedItems(task, obj.Items);

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            if (!int.TryParse(obj.ExternalID, out var taskId))
                return null;

            var deleted = await tasksService.DeleteTaskAsync(taskId);
            return deleted.ToObjectiveExternalDto();
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var isItemsAdding = obj.Items?.Any() ?? false;
            if (!int.TryParse(obj.ExternalID, out var parsedId))
                return null;

            var existingObjectiveModel = await tasksService.GetTaskAsync(parsedId);
            var modelToUpdate = obj.ToModelToUpdate();

            if (isItemsAdding)
            {
                modelToUpdate = await ItemsSynchronizationHelper
                    .UpdateFilesAsync(existingObjectiveModel, modelToUpdate, obj.Items, tasksService.CommonRequests);
            }

            var updatedResult = await tasksService.UpdateTaskAsync(modelToUpdate);
            var parsedResult = updatedResult.ToObjectiveExternalDto();

            if (isItemsAdding)
                parsedResult.Items = updatedResult.Values.Files.ToDtoItems(obj.Items);

            return parsedResult;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await CheckCashedElements();
            return objectives
                .Where(o => o.UpdatedAt >= date)
                .Select(o => o.ExternalID).ToList();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await CheckCashedElements();
            return objectives.Where(o => ids.Contains(o.ExternalID)).ToList();
        }

        private async Task<ICollection<ItemExternalDto>> FindAttachedItems(ObjectBase task, ICollection<ItemExternalDto> items)
        {
            var resultItems = new List<ItemExternalDto>();
            if (task.Values?.BimRef?.ID != null)
            {
                var bim = await bimsService.GetBimLastVersion(task.Values.BimRef.ID.Value.ToString());
                if (bim != null)
                    resultItems.Add(bim.ToItemExternalDto());
            }

            if (task.Values.Files != null)
            {
                var attachedFiles = task.Values.Files.ToDtoItems(items);
                resultItems.AddRange(attachedFiles);
            }

            return resultItems;
        }

        private async Task CheckCashedElements()
        {
            if (objectives == null)
            {
                objectives = new List<ObjectiveExternalDto>();

                var lementTasks = await tasksService.GetAllTasksAsync();
                var files = await bimsService.GetAllBimFilesAsync();

                foreach (var task in lementTasks)
                {
                    // It is necessary to get full info about issue to get last updated info
                    var fullInfoTask = await tasksService.GetTaskAsync(task.ID.Value);
                    var objective = fullInfoTask.ToObjectiveExternalDto();
                    objective.Items = GetIssueFiles(fullInfoTask, files);
                    objectives.Add(objective);
                }
            }
        }

        private ICollection<ItemExternalDto> GetIssueFiles(ObjectBase issue, IEnumerable<ObjectBase> allBims)
        {
            var files = new List<ItemExternalDto>();
            var bimRef = issue.Values.BimRef;
            var bimFile = allBims.FirstOrDefault(b => b.ID == bimRef?.ID)?.ToItemExternalDto();
            if (bimFile != null)
                files.Add(bimFile);

            if (issue.Values.Files != null)
                files.AddRange(issue.Values.Files.ToDtoItems());

            return files;
        }
    }
}
