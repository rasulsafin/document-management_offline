using System.Collections.Generic;

namespace DocumentManagement.Models
{
    public class Project: Entity
    {
        public string Title { get; set; }
        public List<TaskDm> Tasks { get; set; }
        public List<Item> Items { get; set; }
    }
}
