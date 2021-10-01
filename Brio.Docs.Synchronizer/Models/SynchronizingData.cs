using System;
using System.Linq.Expressions;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Synchronization.Models
{
    public class SynchronizingData
    {
        public User User { get; set; }

        public Expression<Func<Project, bool>> ProjectsFilter { get; set; } = project => true;

        public Expression<Func<Objective, bool>> ObjectivesFilter { get; set; } = objective => true;

        internal DateTime Date { get; set; }
    }
}
