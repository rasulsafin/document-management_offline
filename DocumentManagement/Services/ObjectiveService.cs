using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Utils.ReportCreator;

namespace MRS.DocumentManagement.Services
{
    public class ObjectiveService : IObjectiveService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ItemHelper itemHelper;
        private readonly DynamicFieldHelper dynamicFieldHelper;
        private readonly ReportHelper reportHelper = new ReportHelper();

        public ObjectiveService(DMContext context,
            IMapper mapper,
            ItemHelper itemHelper,
            DynamicFieldHelper dynamicFieldHelper)
        {
            this.context = context;
            this.mapper = mapper;
            this.itemHelper = itemHelper;
            this.dynamicFieldHelper = dynamicFieldHelper;
        }

        public async Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data)
        {
            var objective = mapper.Map<Objective>(data);
            await context.Objectives.AddAsync(objective);
            await context.SaveChangesAsync();

            objective.ObjectiveType = await context.ObjectiveTypes.FindAsync(objective.ObjectiveTypeID);

            objective.BimElements = new List<BimElementObjective>();
            foreach (var bim in data.BimElements ?? Enumerable.Empty<BimElementDto>())
            {
                var dbBim = await context.BimElements
                    .Where(x => x.ParentName == bim.ParentName)
                    .Where(x => x.GlobalID == bim.GlobalID)
                    .FirstOrDefaultAsync();
                if (dbBim == null)
                {
                    dbBim = mapper.Map<BimElement>(bim);
                    context.BimElements.Add(dbBim);
                    await context.SaveChangesAsync();
                }

                objective.BimElements.Add(new BimElementObjective
                {
                    ObjectiveID = objective.ID,
                    BimElementID = dbBim.ID,
                });
            }

            objective.Items = new List<ObjectiveItem>();
            foreach (var item in data.Items ?? Enumerable.Empty<ItemDto>())
            {
                await LinkItem(item, objective);
            }

            objective.DynamicFields = new List<DynamicField>();
            foreach (var field in data.DynamicFields ?? Enumerable.Empty<IDynamicFieldDto>())
            {
                await dynamicFieldHelper.AddDynamicFields(field, objective.ID);
            }

            await context.SaveChangesAsync();
            return mapper.Map<ObjectiveToListDto>(objective);
        }

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            var dbObjective = await Get(objectiveID);
            if (dbObjective == null)
                return null;

            var objective = mapper.Map<ObjectiveDto>(dbObjective);
            objective.DynamicFields = new List<IDynamicFieldDto>();

            var listFromDb = dbObjective.DynamicFields;
            foreach (var field in listFromDb)
            {
                var dynamicFieldDto = await dynamicFieldHelper.BuildObjectDynamicField(field);
                objective.DynamicFields.Add(dynamicFieldDto);
            }

            return objective;
        }

        public async Task<bool> GenerateReport(IEnumerable<ID<ObjectiveDto>> objectiveIds, string path, int userID, string projectName)
        {
            int count = 0;
            DateTime date = DateTime.Now.Date;

            var reportCount = await context.ReportCounts.FindAsync(userID);
            if (reportCount != null)
            {
                if (reportCount.Date == date)
                    count = reportCount.Count;
            }
            else
            {
                reportCount = new ReportCount() { UserID = userID, Count = count, Date = date };
                await context.AddAsync(reportCount);
            }

            reportCount.Count = ++count;
            reportCount.Date = date;
            await context.SaveChangesAsync();

            string reportID = $"{date:ddMMyyyy}-{count}";

            List<ObjectiveToReportDto> objectives = new List<ObjectiveToReportDto>();
            var objNum = 1;
            foreach (var objectiveId in objectiveIds)
            {
                var objective = await Get(objectiveId);
                var objectiveToReport = mapper.Map<ObjectiveToReportDto>(objective);
                objectiveToReport.ID = $"{reportID}/{objNum++}";

                foreach (var item in objectiveToReport.Items)
                {
                    var newName = Path.Combine(path, item.RelativePath.TrimStart('\\'));
                    item.RelativePath = newName;
                }

                objectives.Add(objectiveToReport);
            }

            path = Path.Combine(path, $"Отчет {reportID}.docx");
            var xmlDoc = reportHelper.Convert(objectives, path, projectName, reportID, date);

            ReportCreator reportCreator = new ReportCreator();
            reportCreator.CreateReport(xmlDoc, path);

            return true;
        }

        public async Task<IEnumerable<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID)
        {
            var dbProject = await context.Projects.Unsynchronized()
                .Include(x => x.Objectives)
                .ThenInclude(x => x.DynamicFields)
                .Include(x => x.Objectives)
                .ThenInclude(x => x.ObjectiveType)
                .Include(x => x.Objectives)
                .ThenInclude(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)projectID);

            if (dbProject == null)
                return Enumerable.Empty<ObjectiveToListDto>();

            return dbProject.Objectives.Select(x => mapper.Map<ObjectiveToListDto>(x)).ToList();
        }

        public async Task<bool> Remove(ID<ObjectiveDto> objectiveID)
        {
            var objective = await context.Objectives.FindAsync((int)objectiveID);
            if (objective == null)
                return false;
            context.Objectives.Remove(objective);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Update(ObjectiveDto objData)
        {
            var objective = await Get(objData.ID);

            if (objective == null)
                return false;

            objective = mapper.Map(objData, objective);

            var newFields = objData.DynamicFields ?? Enumerable.Empty<IDynamicFieldDto>();
            var currentObjectiveFields = objective.DynamicFields.ToList();
            var fieldsToRemove = currentObjectiveFields.Where(x => newFields.All(f => (int)f.ID != x.ID)).ToList();
            context.DynamicFields.RemoveRange(fieldsToRemove);

            foreach (var field in newFields)
            {
                await dynamicFieldHelper.UpdateDynamicField(field, objective.ID);
            }

            var newBimElements = objData.BimElements ?? Enumerable.Empty<BimElementDto>();
            var currentBimLinks = objective.BimElements.ToList();
            var linksToRemove = currentBimLinks
                .Where(x => !newBimElements.Any(e =>
                    e.ParentName == x.BimElement.ParentName
                    && e.GlobalID == x.BimElement.GlobalID))
                .ToList();
            context.BimElementObjectives.RemoveRange(linksToRemove);

            // Rebuild objective's BimElements
            objective.BimElements.Clear();
            foreach (var bim in newBimElements)
            {
                // See if objective already had this bim element referenced
                var dbBim = currentBimLinks.SingleOrDefault(x => x.BimElement.ParentName == bim.ParentName && x.BimElement.GlobalID == bim.GlobalID);
                if (dbBim != null)
                {
                    objective.BimElements.Add(dbBim);
                }
                else
                {
                    // Bim element was not referenced. Does it exist?
                    var bimElement = await context.BimElements.FirstOrDefaultAsync(x => x.ParentName == bim.ParentName && x.GlobalID == bim.GlobalID);
                    if (bimElement == null)
                    {
                        // Bim element does not exist at all - should be created
                        bimElement = mapper.Map<BimElement>(bim);
                        await context.BimElements.AddAsync(bimElement);
                        await context.SaveChangesAsync();
                    }

                    // Add link between bim element and objective
                    dbBim = new BimElementObjective { BimElementID = bimElement.ID, ObjectiveID = objective.ID };
                    objective.BimElements.Add(dbBim);
                }
            }

            objective.Items = new List<ObjectiveItem>();
            var objectiveItems = context.ObjectiveItems.Where(i => i.ObjectiveID == objective.ID).ToList();
            var itemsToUnlink = objectiveItems.Where(o => (!objData.Items?.Any(i => (int)i.ID == o.ItemID)) ?? true);

            foreach (var item in objData.Items ?? Enumerable.Empty<ItemDto>())
            {
                await LinkItem(item, objective);
            }

            foreach (var item in itemsToUnlink)
            {
                await UnlinkItem(item.ItemID, objective.ID);
            }

            context.Update(objective);
            await context.SaveChangesAsync();
            return true;
        }

        private async Task LinkItem(ItemDto item, Objective objective)
        {
            var dbItem = await itemHelper.CheckItemToLink(context, mapper, item, objective.GetType(), objective.ID);
            if (dbItem == null)
                return;
            objective.Items.Add(new ObjectiveItem
            {
                ObjectiveID = objective.ID,
                ItemID = dbItem.ID,
            });
        }

        private async Task<bool> UnlinkItem(int itemID, int objectiveID)
        {
            var link = await context.ObjectiveItems
                .Where(x => x.ItemID == itemID)
                .Where(x => x.ObjectiveID == objectiveID)
                .FirstOrDefaultAsync();
            if (link == null)
                return false;
            context.ObjectiveItems.Remove(link);
            await context.SaveChangesAsync();

            return true;
        }

        private async Task<Objective> Get(ID<ObjectiveDto> objectiveID)
        {
            var dbObjective = await context.Objectives
               .Include(x => x.Project)
               .Include(x => x.Author)
               .Include(x => x.ObjectiveType)
               .Include(x => x.DynamicFields)
                    .ThenInclude(x => x.ChildrenDynamicFields)
               .Include(x => x.Items)
                    .ThenInclude(x => x.Item)
               .Include(x => x.BimElements)
                    .ThenInclude(x => x.BimElement)
               .FirstOrDefaultAsync(x => x.ID == (int)objectiveID);

            return dbObjective;
        }

    }
}
