using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Services
{
    public class ObjectiveService : IObjectiveService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ItemHelper itemHelper;

        public ObjectiveService(DMContext context, IMapper mapper, ItemHelper itemHelper)
        {
            this.context = context;
            this.mapper = mapper;
            this.itemHelper = itemHelper;
        }

        public async Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data)
        {
            var objective = mapper.Map<Objective>(data);
            context.Objectives.Add(objective);
            await context.SaveChangesAsync();

            objective.ObjectiveType = context.ObjectiveTypes.Find(objective.ObjectiveTypeID);

            objective.BimElements = new List<BimElementObjective>();
            foreach (var bim in data.BimElements ?? Enumerable.Empty<BimElementDto>())
            {
                var dbBim = await context.BimElements
                    .Where(x => x.ItemID == (int)bim.ItemID)
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
                    BimElementID = dbBim.ID
                });
            }

            objective.Items = new List<ObjectiveItem>();
            foreach (var item in data.Items ?? Enumerable.Empty<ItemDto>())
            {
                await LinkItem(item, objective);
            }

            objective.DynamicFields = new List<DynamicField>();
            foreach (var field in data.DynamicFields ?? Enumerable.Empty<DynamicFieldToCreateDto>())
            {
                var dynamicField = mapper.Map<DynamicField>(field);
                dynamicField.ObjectiveID = objective.ID;
                context.DynamicFields.Add(dynamicField);
            }

            await context.SaveChangesAsync();

            return mapper.Map<ObjectiveToListDto>(objective);
        }

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            var dbObjective = await context.Objectives
                .Include(x => x.Project)
                .Include(x => x.Author)
                .Include(x => x.ObjectiveType)
                .Include(x => x.DynamicFields)
                .Include(x => x.Items)
                .ThenInclude(x => x.Item)
                .Include(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)objectiveID);
            if (dbObjective == null)
                return null;
            return mapper.Map<ObjectiveDto>(dbObjective);
        }

        public async Task<IEnumerable<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID)
        {
            var dbProject = await context.Projects
                .Include(x => x.Objectives)
                .ThenInclude(x => x.DynamicFields)
                .Include(x => x.Objectives)
                .ThenInclude(x=>x.ObjectiveType)
                .Include(x => x.Objectives)
                .ThenInclude(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)projectID);

            if (dbProject == null)
                return Enumerable.Empty<ObjectiveToListDto>();

            return dbProject.Objectives.Select(x => mapper.Map<ObjectiveToListDto>(x)).ToList();
        }

        public Task<IEnumerable<DynamicFieldInfoDto>> GetRequiredDynamicFields(ObjectiveTypeDto type)
        {
             throw new NotImplementedException();
            //IEnumerable<DynamicFieldInfoDto> list = Enumerable.Empty<DynamicFieldInfoDto>();
            //return Task.FromResult(list);
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
            var objective = await context.Objectives
                .Include(x => x.Project)
                .Include(x => x.Author)
                .Include(x => x.ObjectiveType)
                .Include(x => x.DynamicFields)
                .Include(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)objData.ID);
            if (objective == null)
                return false;

            objective = mapper.Map(objData, objective);

            var objectiveFields = objective.DynamicFields;
            var newFields = objData.DynamicFields ?? Enumerable.Empty<DynamicFieldDto>();
            var fieldsToRemove = objectiveFields.Where(x => newFields.All(f => (int) f.ID != x.ID)).ToList();
            context.DynamicFields.RemoveRange(fieldsToRemove);

            foreach (var field in newFields)
            {
                var dbField = await context.DynamicFields.FindAsync((int)field.ID);
                if (dbField == null)
                {
                    var dynamicField = mapper.Map<DynamicField>(field);
                    dynamicField.ObjectiveID = objective.ID;
                    await context.DynamicFields.AddAsync(dynamicField);
                }
                else
                {
                    dbField.Key = field.Key;
                    dbField.Type = field.Type;
                    dbField.Value = field.Value;
                    dbField.ObjectiveID = objective.ID;
                    context.DynamicFields.Update(dbField);
                }
            }

            context.Update(objective);
            await context.SaveChangesAsync();

            var newBimElements = objData.BimElements ?? Enumerable.Empty<BimElementDto>();
            var currentBimLinks = objective.BimElements.ToList();
            var linksToRemove = currentBimLinks
                .Where(x => !newBimElements.Any(e => 
                    (int)e.ItemID == x.BimElement.ItemID 
                    && e.GlobalID == x.BimElement.GlobalID)
                ).ToList();
            context.BimElementObjectives.RemoveRange(linksToRemove);

            //rebuild objective's BimElements
            objective.BimElements.Clear();
            foreach (var bim in newBimElements)
            {
                //see if objective already had this bim element referenced
                var dbBim = currentBimLinks.SingleOrDefault(x => x.BimElement.ItemID == (int)bim.ItemID && x.BimElement.GlobalID == bim.GlobalID);
                if (dbBim != null)
                {
                    objective.BimElements.Add(dbBim);
                }
                else 
                {
                    //bim element was not referenced. Does it exist?
                    var bimElement = await context.BimElements.FirstOrDefaultAsync(x => x.ItemID == (int)bim.ItemID && x.GlobalID == bim.GlobalID);
                    if (bimElement == null)
                    {
                        //bim element does not exist at all - should be created
                        bimElement = mapper.Map<BimElement>(bim);
                        await context.BimElements.AddAsync(bimElement);
                        await context.SaveChangesAsync();
                    }
                    //add link between bim element and objective
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
                ItemID = dbItem.ID
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
    }
}
