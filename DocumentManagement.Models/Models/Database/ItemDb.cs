using System.Collections.Generic;

namespace DocumentManagement.Models.Database
{
    public class ItemDb
    {
        public int Id { get; set; }

        public ICollection<ProjectItems> Projects { get; set; }
        public ICollection<TaskItems> Tasks { get; set; }

        public TypeItemDm Type { get; set; }

        public string Path { get; set; }
    }
}
