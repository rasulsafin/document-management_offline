using System;
using System.Collections.Generic;

namespace DocumentManagement.Interface.Models
{
    public struct NewObjective
    {
        public NewObjective(ID<Project> projectID, ID<Objective>? parentObjectiveID, 
            DateTime dueDate, string title, string description, ObjectiveStatus status, 
            ID<ObjectiveType> taskType, IEnumerable<NewDynamicField> dynamicFields, 
            IEnumerable<BimElement> bimElements)
        {
            ProjectID = projectID;
            ParentObjectiveID = parentObjectiveID;
            DueDate = dueDate;
            Title = title;
            Description = description;
            Status = status;
            TaskType = taskType;
            DynamicFields = dynamicFields;
            BimElements = bimElements;
        }

        public ID<Project> ProjectID { get; }
        public ID<Objective>? ParentObjectiveID { get; }
        public DateTime DueDate { get; }
        public string Title { get; }
        public string Description { get; }
        public ObjectiveStatus Status { get; }
        public ID<ObjectiveType> TaskType { get; }
        public IEnumerable<NewDynamicField> DynamicFields { get; }
        public IEnumerable<BimElement> BimElements { get; }
    }
}
