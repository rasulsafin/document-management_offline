using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Services
{
    public class ObjectiveService : IObjectiveService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        public ObjectiveService(DMContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<ID<ObjectiveDto>> Add(ObjectiveToCreateDto data)
        {
            var objective = mapper.Map<Objective>(data);
            context.Objectives.Add(objective);
            await context.SaveChangesAsync();

            objective.BimElements = new List<Database.Models.BimElementObjective>();
            foreach (var bim in data.BimElements ?? Enumerable.Empty<BimElementDto>())
            {
                var dbBim = await context.BimElements
                    .Where(x => x.ItemID == (int)bim.ItemID)
                    .Where(x => x.GlobalID == bim.GlobalID)
                    .FirstOrDefaultAsync();
                if (dbBim == null)
                {
                    dbBim = new Database.Models.BimElement()
                    {
                        ItemID = (int)bim.ItemID,
                        GlobalID = bim.GlobalID
                    };
                    context.BimElements.Add(dbBim);
                }
                objective.BimElements.Add(new Database.Models.BimElementObjective() 
                {
                    ObjectiveID = objective.ID,
                    BimElementID = dbBim.ID
                });
            }

            objective.Items = new List<Database.Models.ObjectiveItem>();
            foreach (var item in data.Items)
            {
                await LinkItem(item, objective);
            }

            objective.DynamicFields = new List<Database.Models.DynamicField>();
            foreach (var field in data.DynamicFields ?? Enumerable.Empty<DynamicFieldToCreateDto>())
            {
                context.DynamicFields.Add(new Database.Models.DynamicField()
                {
                    Key = field.Key,
                    ObjectiveID = objective.ID,
                    Type = field.Type,
                    Value = field.Value
                });                
            }

            await context.SaveChangesAsync();

            return (ID<ObjectiveDto>)objective.ID;
        }

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            var dbObjective = await context.Objectives
                .Include(x => x.Project)
                .Include(x => x.Author)
                .Include(x => x.ObjectiveType)
                .Include(x => x.DynamicFields)
                .Include(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)objectiveID);
            if (dbObjective == null)
                return null;
            return mapper.Map<ObjectiveDto>(dbObjective);
        }

        public async Task<IEnumerable<ObjectiveToListDto>> GetAllObjectives()
        {
            var dbObjectives = await context.Objectives
                .Include(x=> x.ObjectiveType)
                .ToListAsync();
            return dbObjectives.Select(x => mapper.Map<ObjectiveToListDto>(x)).ToList();
        }

        public async Task<IEnumerable<ObjectiveDto>> GetObjectives(ID<ProjectDto> projectID)
        {
            var dbProject = await context.Projects
                .Include(x => x.Objectives)
                .ThenInclude(x => x.DynamicFields)
                .Include(x => x.Objectives)
                .ThenInclude(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)projectID);
            return dbProject.Objectives.Select(x => mapper.Map<ObjectiveDto>(x)).ToList();
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
                //throw new ArgumentException($"Objective with key {objData.ID} not found");

            objective.ObjectiveTypeID = (int)objData.ObjectiveTypeID;
            objective.CreationDate = objData.CreationDate;
            objective.DueDate = objData.DueDate;
            objective.Title = objData.Title;
            objective.Description = objData.Description;
            objective.Status = (int)objData.Status;
            objective.ObjectiveTypeID = (int)objData.ObjectiveTypeID;
            objective.ProjectID = (int)objData.ProjectID;
            objective.AuthorID = (int)objData.AuthorID;
            objective.ParentObjectiveID = objData.ParentObjectiveID.HasValue && objData.ParentObjectiveID.Value.IsValid ? (int?)(objData.ParentObjectiveID.Value) : null;


            var objectiveFields = objective.DynamicFields;
            var newFields = objData.DynamicFields ?? Enumerable.Empty<DynamicFieldDto>();
            var fieldsToRemove = objectiveFields.Where(x => !newFields.Any(f => (int)f.ID == x.ID)).ToList();
            context.DynamicFields.RemoveRange(fieldsToRemove);

            foreach (var field in newFields)
            {
                var dbField = await context.DynamicFields.FindAsync((int)field.ID);
                if (dbField == null)
                {
                    await context.DynamicFields.AddAsync(new Database.Models.DynamicField()
                    {
                        Key = field.Key,
                        Type = field.Type,
                        Value = field.Value,
                        ObjectiveID = objective.ID
                    });
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
                        bimElement = new Database.Models.BimElement() { ItemID = (int)bim.ItemID, GlobalID = bim.GlobalID };
                        await context.BimElements.AddAsync(bimElement);
                        await context.SaveChangesAsync();
                    }
                    //add link between bim element and objective
                    dbBim = new Database.Models.BimElementObjective() { BimElementID = bimElement.ID, ObjectiveID = objective.ID };
                    objective.BimElements.Add(dbBim);
                }
            }

            objective.Items = new List<Database.Models.ObjectiveItem>();
            var objectiveItems = context.ObjectiveItems.Where(i => i.ObjectiveID == objective.ID);
            var itemsToUnlink = objectiveItems.Where(o => objData.Items.Any(i => (int)i.ID == o.ItemID));

            foreach (var item in objData.Items)
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

        private async Task LinkItem(ItemDto item, Database.Models.Objective objective)
        {
            var dbItem = await context.Items
                    .FirstOrDefaultAsync(i => i.ID == (int)item.ID);

            var alreadyLinked = await context.ObjectiveItems
                .AnyAsync(i => i.ItemID == (int)item.ID
                            && i.ObjectiveID == objective.ID);

            if (alreadyLinked)
                return;

            if (dbItem == null)
            {
                dbItem = new Database.Models.Item
                {
                    ID = (int)item.ID,
                    ItemType = (int)item.ItemType,
                    ExternalItemId = item.ExternalItemId
                };
                context.Items.Add(dbItem);
            }
            objective.Items.Add(new Database.Models.ObjectiveItem
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
