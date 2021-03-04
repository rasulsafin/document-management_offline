using System;
using System.Linq.Expressions;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Synchronization.Models
{
    public class SynchronizingData
    {
        public DMContext Context { get; set; }

        public User User { get; set; }

        public Expression<Func<Project, bool>> ProjectsFilter { get; set; } = project => true;

        public Expression<Func<Objective, bool>> ObjectivesFilter { get; set; } = objective => true;
    }
}
