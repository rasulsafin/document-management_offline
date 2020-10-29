using System.Collections.Generic;

namespace DocumentManagement.Models.Database
{
    public class ProjectDb
    {
        public int Id { get; set; }

        public ICollection<ProjectUsers> Users { get; set; } //users, that have access to that value

        public string Title { get; set; }
        public ICollection<TaskDmDb> Tasks { get; set; }
        public ICollection<ProjectItems> Items { get; set; }
    }
}
