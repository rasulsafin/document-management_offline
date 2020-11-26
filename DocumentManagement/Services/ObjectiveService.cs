using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using System.Linq;

namespace MRS.DocumentManagement.Services
{
    internal class ObjectiveService : IObjectiveService
    {
        private readonly DMContext context;

        public ObjectiveService(DMContext context)
        {
            this.context = context;
        }

        private static Objective MapObjectiveFromDB(Database.Models.Objective ob)
        {
            return new Objective()
            {
                ID = (ID<Objective>)ob.ID,
                Author = new User((ID<User>)ob.Author.ID, ob.Author.Login, ob.Author.Name),
                CreationDate = ob.CreationDate,
                Description = ob.Description,
                Status = (ObjectiveStatus)ob.Status,
                ProjectID = (ID<Project>)ob.ProjectID,
                ParentObjectiveID = ob.ParentObjectiveID.HasValue
                    ? ((ID<Objective>?)ob.ParentObjectiveID)
                    : null,
                DueDate = ob.DueDate,
                TaskType = new ObjectiveType()
                {
                    ID = (ID<ObjectiveType>)ob.ObjectiveType.ID,
                    Name = ob.ObjectiveType.Name
                },
                Title = ob.Title,
                DynamicFields = ob.DynamicFields
                    .Select(x => new DynamicField()
                    {
                        ID = (ID<DynamicField>)x.ID,
                        Key = x.Key,
                        Type = x.Type,
                        Value = x.Value
                    }).ToList(),
                BimElements = ob.BimElements
                    .Select(x => new BimElement()
                    {
                        ItemID = (ID<Item>)x.BimElement.ItemID,
                        GlobalID = x.BimElement.GlobalID
                    }).ToList()
            };
        }

        public async Task<ID<Objective>> Add(ObjectiveToCreate data)
        {
            var objective = new Database.Models.Objective()
            {
                AuthorID = data.AuthorID.HasValue ? new int?((int)data.AuthorID) : null,
                CreationDate = data.CreationDate,
                DueDate = data.DueDate,
                Description = data.Description,
                Title = data.Title,
                ObjectiveTypeID = (int)data.TaskType,
                Status = (int)data.Status,
                ParentObjectiveID = data.ParentObjectiveID.HasValue ? new int?((int)data.ParentObjectiveID) : null,
                ProjectID = (int)data.ProjectID,
            };

            context.Objectives.Add(objective);
            await context.SaveChangesAsync();

            objective.BimElements = new List<Database.Models.BimElementObjective>();
            foreach (var bim in data.BimElements ?? Enumerable.Empty<BimElement>())
            {
                var dbBim = await context.BimElements
                    .Where(x => x.ItemID == (int)bim.ItemID)
                    .Where(x => x.GlobalID == bim.GlobalID)
                    .FirstOrDefaultAsync();
                if (dbBim == null)
                {
                    context.BimElements.Add(new Database.Models.BimElement() 
                    {
                        ItemID = (int)bim.ItemID,
                        GlobalID = bim.GlobalID
                    });
                }
                objective.BimElements.Add(new Database.Models.BimElementObjective() 
                {
                    ObjectiveID = objective.ID,
                    BimElementID = dbBim.ID
                });
            }

            objective.DynamicFields = new List<Database.Models.DynamicField>();
            foreach (var field in data.DynamicFields ?? Enumerable.Empty<DynamicFieldToCreate>())
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

            return (ID<Objective>)objective.ID;
        }

        public async Task<Objective> Find(ID<Objective> objectiveID)
        {
            var dbObjective = await context.Objectives
                .Include(x => x.DynamicFields)
                .Include(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)objectiveID);
            if (dbObjective == null)
                return null;
            return MapObjectiveFromDB(dbObjective);
        }

        public async Task<IEnumerable<Objective>> GetAllObjectives()
        {
            var dbObjectives = await context.Objectives
                .Include(x => x.DynamicFields)
                .Include(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .ToListAsync();
            return dbObjectives.Select(x => MapObjectiveFromDB(x)).ToList();
        }

        public async Task<IEnumerable<Objective>> GetObjectives(ID<Project> projectID)
        {
            var dbProject = await context.Projects
                .Include(x => x.Objectives)
                .ThenInclude(x => x.DynamicFields)
                .Include(x => x.Objectives)
                .ThenInclude(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)projectID);
            return dbProject.Objectives.Select(x => MapObjectiveFromDB(x)).ToList();
        }

        public Task<IEnumerable<DynamicFieldInfo>> GetRequiredDynamicFields()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Remove(ID<Objective> objectiveID)
        {
            var objective = await context.Objectives.FindAsync((int)objectiveID);
            if (objective == null)
                return false;
            context.Objectives.Remove(objective);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task Update(Objective objData)
        {
            var objective = await context.Objectives
                .Include(x => x.DynamicFields)
                .Include(x => x.BimElements)
                .ThenInclude(x => x.BimElement)
                .FirstOrDefaultAsync(x => x.ID == (int)objData.ID);
            if (objective == null)
                throw new ArgumentException($"Objective with key {objData.ID} not found");
            objective.AuthorID = (int)objData.Author.ID;
            objective.ObjectiveTypeID = (int)objData.TaskType.ID;
            objective.ProjectID = (int)objData.ProjectID;
            objective.ParentObjectiveID = objData.ParentObjectiveID.HasValue ? (int?)(objData.ParentObjectiveID.Value) : null;
            objective.CreationDate = objData.CreationDate;
            objective.DueDate = objData.DueDate;
            objective.Title = objData.Title;
            objective.Description = objData.Description;
            objective.Status = (int)objData.Status;
            objective.ObjectiveTypeID = (int)objData.TaskType.ID;

            var objectiveFields = objective.DynamicFields;
            var newFields = objData.DynamicFields ?? Enumerable.Empty<DynamicField>();
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

            var newBimElements = objData.BimElements ?? Enumerable.Empty<BimElement>();
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

            context.Update(objective);
            await context.SaveChangesAsync();
        }
    }
}
