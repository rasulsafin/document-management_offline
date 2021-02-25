using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Synchronizer.Models
{
    public class SynchronizingData
    {
        public DbSet<Project> Projects { get; set; }

        public DbSet<Objective> Objectives { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<DynamicField> DynamicFields { get; set; }
    }
}
