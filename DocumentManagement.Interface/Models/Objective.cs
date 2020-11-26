using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Models
{
    public class Objective
    {
        public ID<Objective> ID { get; set; }
        public ID<Project> ProjectID { get; set; }
        public ID<Objective>? ParentObjectiveID { get; set; }
        public User Author { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ObjectiveStatus Status { get; set; }
        public ObjectiveType TaskType { get; set; }
        public IEnumerable<DynamicField> DynamicFields { get; set; }
        public IEnumerable<BimElement> BimElements { get; set; }
    }
}
