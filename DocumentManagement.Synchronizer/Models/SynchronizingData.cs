using System;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Synchronizer.Models
{
    public class SynchronizingData
    {
        public DbSet<Project> Projects { get; set; }

        public DbSet<Objective> Objectives { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<MRS.DocumentManagement.Database.Models.Synchronization> Synchronizations { get; set; }

        public DbSet<DynamicField> DynamicFields { get; set; }

        public Predicate<Project> ProjectsFilter { get; set; }

        public Predicate<Objective> ObjectivesFilter { get; set; }
    }
}
