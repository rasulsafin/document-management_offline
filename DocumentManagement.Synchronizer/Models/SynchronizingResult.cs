using System.Collections.Generic;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Synchronizer.Models
{
    public class SynchronizingResult
    {
        public List<(SynchronizingStatus, Project)> Projects { get; set; }

        public List<(SynchronizingStatus, Objective)> Objectives { get; set; }

        public List<(SynchronizingStatus, Item)> Items { get; set; }

        public List<(SynchronizingStatus, DynamicField)> DynamicFields { get; set; }
    }
}
