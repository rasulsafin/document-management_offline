using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.General.Utils.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Filters;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Utility.Extensions;
using MRS.DocumentManagement.Utility.Pagination;
using MRS.DocumentManagement.Utils.ReportCreator;

namespace MRS.DocumentManagement.Services
{
    public class ObjectiveService : IObjectiveService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ItemHelper itemHelper;
        private readonly DynamicFieldHelper dynamicFieldHelper;
        private readonly ILogger<ObjectiveService> logger;
        private readonly ReportHelper reportHelper = new ReportHelper();

        public ObjectiveService(DMContext context,
            IMapper mapper,
            ItemHelper itemHelper,
            DynamicFieldHelper dynamicFieldHelper,
            ILogger<ObjectiveService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.itemHelper = itemHelper;
            this.dynamicFieldHelper = dynamicFieldHelper;
            this.logger = logger;
            logger.LogTrace("ObjectiveService created");
        }

        public async Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with data: {@Data}", data);
            try
            {
                var objective = mapper.Map<Objective>(data);
                var location = objective.Location;
                objective.Location = null;
                logger.LogTrace("Mapped data: {@Objective}", objective);
                await context.Objectives.AddAsync(objective);
                await context.SaveChangesAsync();

                objective.ObjectiveType = await context.ObjectiveTypes.FindAsync(objective.ObjectiveTypeID);
                logger.LogTrace("ObjectiveType: {@ObjectiveType}", objective.ObjectiveType);

                objective.BimElements = new List<BimElementObjective>();
                foreach (var bim in data.BimElements ?? Enumerable.Empty<BimElementDto>())
                {
                    logger.LogTrace("Bim element: {@Bim}", bim);
                    var dbBim = await context.BimElements
                        .Where(x => x.ParentName == bim.ParentName)
                        .Where(x => x.GlobalID == bim.GlobalID)
                        .FirstOrDefaultAsync();
                    logger.LogDebug("Found BIM element: {@DBBim}", dbBim);

                    if (dbBim == null)
                    {
                        dbBim = mapper.Map<BimElement>(bim);
                        await context.BimElements.AddAsync(dbBim);
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
                foreach (var field in data.DynamicFields ?? Enumerable.Empty<DynamicFieldDto>())
                {
                    await dynamicFieldHelper.AddDynamicFields(field, objective.ID);
                }

                if (location != null)
                    await LinkLocation(data.Location?.Item, objective, location);

                await context.SaveChangesAsync();
                return mapper.Map<ObjectiveToListDto>(objective);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't add objective {@Data}", data);
                throw;
            }
        }

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with objectiveID: {@ObjectiveID}", objectiveID);
            try
            {
                var dbObjective = await Get(objectiveID);
                logger.LogDebug("Found: {@DBObjective}", dbObjective);
                var objective = mapper.Map<ObjectiveDto>(dbObjective);
                objective.DynamicFields = new List<DynamicFieldDto>();

                var listFromDb = dbObjective.DynamicFields;
                foreach (var field in listFromDb)
                {
                    var dynamicFieldDto = await dynamicFieldHelper.BuildObjectDynamicField(field);
                    objective.DynamicFields.Add(dynamicFieldDto);
                }

                logger.LogDebug("Created DTO: {@Objective}", objective);
                return objective;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get objective with key {ObjectiveID}", objectiveID);
                throw;
            }
        }

        public async Task<ObjectiveReportCreationResultDto> GenerateReport(IEnumerable<ID<ObjectiveDto>> objectiveIds, string path, int userID, string projectName)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogInformation(
                "GenerateReport started for user {UserId} with path = {Path}, projectName = {ProjectName} objectiveIds: {@ObjectiveIDs}",
                userID,
                path,
                projectName,
                objectiveIds);
            try
            {
                if (objectiveIds == null)
                    throw new ArgumentException("Cannot create report without objectives");

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
                logger.LogDebug("Report Count updating: {@ReportCount}", reportCount);
                await context.SaveChangesAsync();

                string reportID = $"{date:yyyyMMdd}-{count}";

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

                logger.LogDebug("Objectives for report: {@Objectives}", objectives);
                var reportDir = Path.Combine(path, "Reports");
                Directory.CreateDirectory(reportDir);
                path = Path.Combine(reportDir, $"Отчет {reportID}.docx");
                var xmlDoc = reportHelper.Convert(objectives, path, projectName, reportID, date);
                logger.LogDebug("XML created: {@XDocument}", xmlDoc);

                ReportCreator reportCreator = new ReportCreator();
                reportCreator.CreateReport(xmlDoc, path);
                logger.LogInformation("Report created ({Path})", path);

                return new ObjectiveReportCreationResultDto()
                {
                    ReportPath = path,
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't create report");
                throw;
            }
        }

        public async Task<PagedListDto<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID, ObjectiveFilterParameters filter)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectives started with projectID: {@ProjectID}", projectID);
            try
            {

                var dbProject = await context.Projects.Unsynchronized()
                    .FindOrThrowAsync(x => x.ID, (int)projectID);
                logger.LogDebug("Found project: {@DBProject}", dbProject);

                var allObjectives = context.Objectives
                    .AsNoTracking()
                    .Unsynchronized()
                                    .Where(x => x.ProjectID == dbProject.ID)
                                    .Where(x => filter.TypeId == 0 || filter.TypeId == null || x.ObjectiveTypeID == filter.TypeId)
                                    .Where(x => string.IsNullOrEmpty(filter.BimElementParentName) || x.BimElements.Any(e => e.BimElement.ParentName == filter.BimElementParentName))
                                    .Where(x => string.IsNullOrEmpty(filter.BimElementGuid) || x.BimElements.Any(e => e.BimElement.GlobalID == filter.BimElementGuid));

                var totalCount = allObjectives != null ? await allObjectives.CountAsync() : 0;
                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                var objectives = await allObjectives?
                    .ByPages(x => x.ID, filter.PageNumber, filter.PageSize)
                    .Include(x => x.ObjectiveType)
                    .Include(x => x.BimElements)
                            .ThenInclude(x => x.BimElement)
                    .Include(x => x.Location)
                            .ThenInclude(x => x.Item)
                    .Select(x => mapper.Map<ObjectiveToListDto>(x))
                    .ToListAsync();

                return new PagedListDto<ObjectiveToListDto>()
                {
                    Items = objectives ?? Enumerable.Empty<ObjectiveToListDto>(),
                    PageData = new PagedDataDto()
                    {
                        CurrentPage = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                    },
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by project key {ProjectID}", projectID);
                throw;
            }
        }

        public async Task<bool> Remove(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Remove started with objectiveID: {@ObjectiveID}", objectiveID);
            try
            {
                var objective = await context.Objectives.FindOrThrowAsync((int)objectiveID);
                logger.LogDebug("Found objective: {@Objective}", objective);
                context.Objectives.Remove(objective);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove objective with key {ObjectiveID}", objectiveID);
                throw;
            }
        }

        public async Task<bool> Update(ObjectiveDto objData)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with objData: {@ObjData}", objData);
            try
            {
                var objective = await Get(objData.ID);

                objective = mapper.Map(objData, objective);

                var newFields = objData.DynamicFields ?? Enumerable.Empty<DynamicFieldDto>();
                var currentObjectiveFields = objective.DynamicFields.ToList();
                var fieldsToRemove = currentObjectiveFields.Where(x => newFields.All(f => (int)f.ID != x.ID)).ToList();
                logger.LogDebug(
                    "Objective's ({ID}) dynamic fields to remove: {@FieldsToRemove}",
                    objData.ID,
                    fieldsToRemove);
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
                logger.LogDebug(
                    "Objective's ({ID}) BIM elements links to remove: {@LinksToRemove}",
                    objData.ID,
                    linksToRemove);
                context.BimElementObjectives.RemoveRange(linksToRemove);

                // Rebuild objective's BimElements
                objective.BimElements.Clear();
                foreach (var bim in newBimElements)
                {
                    // See if objective already had this bim element referenced
                    var dbBim = currentBimLinks.SingleOrDefault(x => x.BimElement.ParentName == bim.ParentName && x.BimElement.GlobalID == bim.GlobalID);
                    logger.LogDebug("Found dbBim: {@DBBim}", dbBim);
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
                            logger.LogDebug("Adding BIM element: {@BimElement}", bimElement);
                            await context.BimElements.AddAsync(bimElement);
                            await context.SaveChangesAsync();
                        }

                        // Add link between bim element and objective
                        dbBim = new BimElementObjective { BimElementID = bimElement.ID, ObjectiveID = objective.ID };
                        objective.BimElements.Add(dbBim);
                    }
                }

                objective.Items ??= new List<ObjectiveItem>();
                var objectiveItems = context.ObjectiveItems.Where(i => i.ObjectiveID == objective.ID).ToList();
                var itemsToUnlink = objectiveItems.Where(o => (!objData.Items?.Any(i => (int)i.ID == o.ItemID)) ?? true);
                logger.LogDebug(
                "Objective's ({ID}) item links to remove: {@ItemsToUnlink}",
                objData.ID,
                itemsToUnlink);

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
            catch (Exception e)
            {
                logger.LogError(e, "Can't update objective {@ObjData}", objData);
                throw;
            }
        }

        private async Task LinkItem(ItemDto item, Objective objective)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("LinkItem started for objective {ID} with item: {@Item}", objective.ID, item);
            var dbItem = await itemHelper.CheckItemToLink(context, mapper, item, objective);
            logger.LogDebug("CheckItemToLink returned {@DBItem}", dbItem);
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
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("UnlinkItem started for objective {ID} with item: {ItemID}", objectiveID, itemID);
            var link = await context.ObjectiveItems
                .Where(x => x.ItemID == itemID)
                .Where(x => x.ObjectiveID == objectiveID)
                .FirstOrDefaultAsync();
            logger.LogDebug("Found link {@Link}", link);
            if (link == null)
                return false;
            context.ObjectiveItems.Remove(link);
            await context.SaveChangesAsync();

            return true;
        }

        private async Task<Objective> Get(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Get started for objective {ID}", objectiveID);
            var dbObjective = await context.Objectives
               .Include(x => x.Project)
               .Include(x => x.Author)
               .Include(x => x.ObjectiveType)
               .Include(x => x.Location)
                    .ThenInclude(x => x.Item)
               .Include(x => x.DynamicFields)
                    .ThenInclude(x => x.ChildrenDynamicFields)
               .Include(x => x.Items)
                    .ThenInclude(x => x.Item)
               .Include(x => x.BimElements)
                    .ThenInclude(x => x.BimElement)
               .FindOrThrowAsync(x => x.ID, (int)objectiveID);

            logger.LogDebug("Found objective: {@DBObjective}", dbObjective);

            return dbObjective;
        }

        private async Task LinkLocation(ItemDto locationItemDto, Objective objective, Location location)
        {
            if (locationItemDto != null)
            {
                objective.Project ??= await context.Projects.Include(x => x.Items)
                   .FindOrThrowAsync(x => x.ID, objective.ProjectID);

                var locationItem = await itemHelper.CheckItemToLink(
                    context,
                    mapper,
                    locationItemDto,
                    objective.Project);

                if (locationItem != null)
                    objective.Project.Items.Add(locationItem);
                else
                    locationItem = await context.FindOrThrowAsync<Item>((int)locationItemDto.ID);

                objective.Location = location;
                objective.Location.Item = locationItem;
            }
        }
    }
}
