using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
{
    public class LementProObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly TasksService tasksService;
        private readonly BimsService bimsService;

        public LementProObjectivesSynchronizer(LementProConnectionContext context)
        {
            tasksService = context.TasksService;
            bimsService = context.BimsService;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var lementIssue = obj.ToModelToCreate();
            if (obj.Items?.Any() ?? false)
            {
                var fileIds = await UploadFiles(obj.Items);
                lementIssue.FileIds = fileIds;
            }

            var createResult = await tasksService.CreateTask(lementIssue);
            if (!createResult.IsSuccess.GetValueOrDefault())
                return null;

            // Wait for creating
            await Task.Delay(3000);

            var task = await tasksService.GetTaskAsync(createResult.ID.Value);
            var parsedToDto = task.ToObjectiveExternalDto();
            parsedToDto.Items = await FindAttachedItems(task);
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
            var modelToUpdate = obj.ToModelToUpdate();

            var updatedModel = await tasksService.UpdateTaskAsync(modelToUpdate);
            return updatedModel.ToObjectiveExternalDto();
        }

        private async Task<IEnumerable<int>> UploadFiles(ICollection<ItemExternalDto> items)
        {
            var fileIds = new List<int>();
            var existingFiles = items.Where(i => !string.IsNullOrWhiteSpace(i.ExternalID));
            var filesToUpload = items.Where(i => string.IsNullOrWhiteSpace(i.ExternalID));

            foreach (var file in filesToUpload)
            {
                var uploaded = await tasksService.CommonRequests.AddFileAsync(file.FileName, file.FullPath);
                if (uploaded.IsSuccess.GetValueOrDefault())
                    fileIds.Add(uploaded.ID.Value);
            }

            foreach (var file in existingFiles)
            {
                if (int.TryParse(file.ExternalID, out var parsed))
                    fileIds.Add(parsed);
            }

            return fileIds;
        }

        private async Task<ICollection<ItemExternalDto>> FindAttachedItems(ObjectBase task)
        {
            var items = new List<ItemExternalDto>();
            if (task.Values?.BimRef?.ID != null)
            {
                var bim = await bimsService.GetBimLastVersion(task.Values.BimRef.ID.Value.ToString());
                if (bim != null)
                    items.Add(bim.ToItemExternalDto());
            }

            task.Values.Files.ForEach(f => items.Add(f.ToItemExternalDto()));

            return items;
        }
    }
}
