using System;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Synchronizer.Models
{
    public class SynchronizingData
    {
        public DMContext Context { get; set; }

        public User User { get; set; }

        public Predicate<Project> ProjectsFilter { get; set; } = project => true;

        public Predicate<Objective> ObjectivesFilter { get; set; } = objective => true;
    }
}
