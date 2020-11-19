using System.Collections.Generic;

namespace DocumentManagement.Database.Models
{
    public class Project
    {
        public int ID { get; set; }
        
        public string Title { get; set; }

        public ICollection<Objective> Objectives { get; set; }
        public ICollection<UserProject> Users { get; set; }
        public ICollection<ProjectItem> Items { get; set; }
    }
}
