using System;
using System.Collections.Generic;

namespace DocumentManagement.Models
{
    public class TaskDm : Entity
    {
        public string Index { get; set; }

        public TaskType Type { get; set; }
        public Status Status { get; set; }

        public User Author { get; set; }
        public string Title { get; set; }
        public string Descriptions { get; set; }
        public DateTime Date { get; set; }
        public (float, float, float) Location { get; set; }


        public List<Item> Items { get; set; }
        public List<TaskDm> Tasks { get; set; }

        public string Element { get; set; }
    }
}
