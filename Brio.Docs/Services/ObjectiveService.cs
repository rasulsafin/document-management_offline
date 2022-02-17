using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Sorts;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Extensions;
using Brio.Docs.Utility.Pagination;
using Brio.Docs.Utility.Sorting;
using Brio.Docs.Utils.ReportCreator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class ObjectiveService : IObjectiveService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ItemsHelper itemHelper;
        private readonly DynamicFieldsHelper dynamicFieldHelper;
        private readonly BimElementsHelper bimElementHelper;
        private readonly ILogger<ObjectiveService> logger;
        private readonly ReportHelper reportHelper = new ReportHelper();
        private readonly QueryMapper<Objective> queryMapper;

        public ObjectiveService(DMContext context,
            IMapper mapper,
            ItemsHelper itemHelper,
            DynamicFieldsHelper dynamicFieldHelper,
            BimElementsHelper bimElementHelper,
            ILogger<ObjectiveService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.itemHelper = itemHelper;
            this.dynamicFieldHelper = dynamicFieldHelper;
            this.bimElementHelper = bimElementHelper;
            this.logger = logger;
            logger.LogTrace("ObjectiveService created");

            queryMapper = new QueryMapper<Objective>(new QueryMapperConfiguration { IsCaseSensitive = false, IgnoreNotMappedFields = false });
            queryMapper.AddMap(nameof(ObjectiveToListDto.Status), x => x.Status);
            queryMapper.AddMap(nameof(ObjectiveToListDto.Title), x => x.TitleToLower);
            queryMapper.AddMap(nameof(Objective.CreationDate), x => x.CreationDate);
            queryMapper.AddMap(nameof(Objective.UpdatedAt), x => x.UpdatedAt);
            queryMapper.AddMap(nameof(Objective.DueDate), x => x.DueDate);
            queryMapper.AddMap("CreationDateDateOnly", x => x.CreationDate.Date);
            queryMapper.AddMap("UpdatedAtDateOnly", x => x.UpdatedAt.Date);
            queryMapper.AddMap("DueDateDateOnly", x => x.DueDate.Date);
        }

        public async Task<ObjectiveToListDto> Add(ObjectiveToCreateDto objectiveToCreate)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with data: {@Data}", objectiveToCreate);
            try
            {
                var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
                logger.LogTrace("Mapped data: {@Objective}", objectiveToSave);
                await context.Objectives.AddAsync(objectiveToSave);
                await context.SaveChangesAsync();

                await bimElementHelper.AddBimElementsAsync(objectiveToCreate.BimElements, objectiveToSave);
                await itemHelper.AddItemsAsync(objectiveToCreate.Items, objectiveToSave);
                await dynamicFieldHelper.AddDynamicFieldsAsync(objectiveToCreate.DynamicFields, objectiveToSave);
                await AddLocationAsync(objectiveToCreate.Location, objectiveToSave);

                return mapper.Map<ObjectiveToListDto>(objectiveToSave);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add objective {@Data}", objectiveToCreate);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<Objective> AddLocationAsync(LocationDto locationDto, Objective objective)
        {
            var location = mapper.Map<Location>(locationDto);
            if (location != null)
            {
                var locationItemDto = locationDto?.Item;
                if (locationItemDto != null)
                {
                    objective.Project ??= await context.Projects.Include(x => x.Items)
                       .FindOrThrowAsync(x => x.ID, objective.ProjectID);

                    var locationItem = await itemHelper.CheckItemToLink(
                        locationItemDto,
                        new ProjectItemContainer(objective.Project));

                    if (locationItem != null)
                        objective.Project.Items.Add(locationItem);
                    else
                        locationItem = await context.FindOrThrowAsync<Item>((int)locationItemDto.ID);

                    objective.Location = location;
                    objective.Location.Item = locationItem;
                }

                await context.SaveChangesAsync();
            }

            return objective;
        }

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with objectiveID: {@ObjectiveID}", objectiveID);
            try
            {
                var dbObjective = await GetOrThrowAsync(objectiveID);
                logger.LogDebug("Found: {@DBObjective}", dbObjective);
                var objective = mapper.Map<ObjectiveDto>(dbObjective);
                logger.LogDebug("Created DTO: {@Objective}", objective);
                return objective;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get objective with key {ObjectiveID}", objectiveID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
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
                    throw new ArgumentValidationException("Cannot create report without objectives");

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
                    var objective = await GetOrThrowAsync(objectiveId);
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
                if (ex is ArgumentValidationException || ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<PagedListDto<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID, ObjectiveFilterParameters filter, SortParameters sort)
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
                                    .Where(x => x.ProjectID == dbProject.ID);

                if (filter.TypeIds != null && filter.TypeIds.Count > 0)
                    allObjectives = allObjectives.Where(x => filter.TypeIds.Contains(x.ObjectiveTypeID));

                if (!string.IsNullOrEmpty(filter.BimElementGuid))
                    allObjectives = allObjectives.Where(x => x.BimElements.Any(e => e.BimElement.GlobalID == filter.BimElementGuid));

                if (!string.IsNullOrWhiteSpace(filter.TitlePart))
                    allObjectives = allObjectives.Where(x => x.TitleToLower.Contains(filter.TitlePart.ToLower()));

                if (filter.Statuses != null && filter.Statuses.Count > 0)
                    allObjectives = allObjectives.Where(x => filter.Statuses.Contains(x.Status));

                if (filter.CreatedBefore.HasValue)
                    allObjectives = allObjectives.Where(x => x.CreationDate <= filter.CreatedBefore.Value);

                if (filter.CreatedAfter.HasValue)
                    allObjectives = allObjectives.Where(x => x.CreationDate >= filter.CreatedAfter.Value);

                if (filter.UpdatedBefore.HasValue)
                    allObjectives = allObjectives.Where(x => x.UpdatedAt <= filter.UpdatedBefore.Value);

                if (filter.UpdatedAfter.HasValue)
                    allObjectives = allObjectives.Where(x => x.UpdatedAt >= filter.UpdatedAfter.Value);

                if (filter.FinishedBefore.HasValue)
                    allObjectives = allObjectives.Where(x => x.DueDate <= filter.FinishedBefore.Value);

                if (filter.FinishedAfter.HasValue)
                    allObjectives = allObjectives.Where(x => x.DueDate >= filter.FinishedAfter.Value);

                if (filter.ExceptChildrenOf.HasValue && filter.ExceptChildrenOf.Value != 0)
                {
                    var obj = await context.Objectives
                        .AsNoTracking()
                        .Unsynchronized()
                        .Where(x => x.ProjectID == dbProject.ID)
                        .FirstOrDefaultAsync(o => o.ID == (int)filter.ExceptChildrenOf);

                    var childrenIds = new List<int>();
                    if (obj != null)
                        await GetAllObjectiveIds(obj, childrenIds);

                    allObjectives = allObjectives.Where(x => !childrenIds.Contains(x.ID));
                }

                var totalCount = allObjectives != null ? await allObjectives.CountAsync() : 0;
                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                var objectives = await allObjectives?
                    .SortWithParameters(sort, queryMapper, x => x.CreationDate)
                    .ByPages(filter.PageNumber, filter.PageSize)
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
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ObjectiveToLocationDto>> GetObjectivesWithLocation(ID<ProjectDto> projectID, string itemName)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectivesWithLocation started with projectID: {@ProjectID}", projectID);
            try
            {
                var dbProject = await context.Projects.Unsynchronized()
                    .FindOrThrowAsync(x => x.ID, (int)projectID);
                logger.LogDebug("Found project: {@DBProject}", dbProject);

                var objectivesWithLocations = await context.Objectives
                                    .AsNoTracking()
                                    .Unsynchronized()
                                    .Where(x => x.ProjectID == dbProject.ID)
                                    .Include(x => x.Location)
                                        .ThenInclude(x => x.Item)
                                    .Where(x => x.Location != null && x.Location.Item.Name == itemName)
                                    .Select(x => mapper.Map<ObjectiveToLocationDto>(x))
                                    .ToListAsync();

                return objectivesWithLocations;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by project key {ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
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
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> Update(ObjectiveDto objectiveDto)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with objData: {@ObjData}", objectiveDto);
            try
            {
                var objectiveFromDb = await context.Objectives.FindOrThrowAsync((int)objectiveDto.ID);
                objectiveFromDb = mapper.Map(objectiveDto, objectiveFromDb);

                await dynamicFieldHelper.UpdateDynamicFieldsAsync(objectiveDto.DynamicFields, objectiveFromDb.ID);
                await bimElementHelper.UpdateBimElementsAsync(objectiveDto.BimElements, objectiveFromDb.ID);
                await itemHelper.UpdateItemsAsync(objectiveDto.Items, objectiveFromDb);

                context.Update(objectiveFromDb);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't update objective {@ObjData}", objectiveDto);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<SubobjectiveDto>> GetObjectivesByParent(ID<ObjectiveDto> parentID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectivesByParent started with parentID: {@parentID}", parentID);
            try
            {
                var objectivesWithParent = await context.Objectives
                                    .AsNoTracking()
                                    .Unsynchronized()
                                    .Where(x => x.ParentObjectiveID == (int)parentID)
                                    .OrderBy(x => x.CreationDate)
                                    .Select(x => mapper.Map<SubobjectiveDto>(x))
                                    .ToListAsync();

                return objectivesWithParent;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by parentID key {parentID}", parentID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        private async Task<Objective> GetOrThrowAsync(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Get started for objective {ID}", objectiveID);
            var dbObjective = await context.Objectives
                .Unsynchronized()
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

        private async Task GetAllObjectiveIds(Objective obj, List<int> ids)
        {
            ids.Add(obj.ID);

            var children = await context.Objectives
                .Unsynchronized()
                .Where(x => x.ParentObjectiveID == obj.ID)
                .ToListAsync();

            foreach (var child in children)
                await GetAllObjectiveIds(child, ids);
        }
    }
}
