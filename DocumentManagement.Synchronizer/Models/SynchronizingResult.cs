using System.Collections.Generic;
using MRS.DocumentManagement.Database;

namespace MRS.DocumentManagement.Synchronization.Models
{
    public class SynchronizingResult
    {
        public SynchronizingStatus Status { get; set; }

        public ISynchronizableBase Object { get; set; }

        //public List<(SynchronizingStatus, ISynchronizableBase)> Result { get; set; }

       // public List<(SynchronizingStatus, Project)> Projects { get; set; }

        //public List<(SynchronizingStatus, Objective)> Objectives { get; set; }

        //public List<(SynchronizingStatus, Item)> Items { get; set; }

        //public List<(SynchronizingStatus, DynamicField)> DynamicFields { get; set; }
    }
}
