using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Models
{
    public struct ObjectiveToCreate
    {
        public ID<User>? AuthorID { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public ID<Project> ProjectID { get; set; }
        public ID<Objective>? ParentObjectiveID { get; set; }
        
        public string Title { get; set; }
        public string Description { get; set; }
        public ObjectiveStatus Status { get; set; }
        public ID<ObjectiveType> TaskType { get; set; }
        
        public IEnumerable<DynamicFieldToCreate> DynamicFields { get; set; }
        public IEnumerable<BimElement> BimElements { get; set; }
    }
}
