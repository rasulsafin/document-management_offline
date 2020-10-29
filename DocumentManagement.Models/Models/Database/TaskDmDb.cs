using System;
using System.Collections.Generic;

namespace DocumentManagement.Models.Database
{
    public class TaskDmDb 
    {
        public int Id { get; set; }
        public string Index { get; set; }

        public int ProjectId { get; set; }
        public ProjectDb Project { get; set; }

        public int? ParentTaskId { get; set; }
        public TaskDmDb ParentTask { get; set; }

        public TaskType Type { get; set; }
        public Status Status { get; set; }

        public int UserId { get; set; }
        public UserDb Author { get; set; }
        public string Title { get; set; }
        public string Descriptions { get; set; }
        public DateTime Date { get; set; }
        public float LocationX { get; set; }
        public float LocationY { get; set; }
        public float LocationZ { get; set; }
        

        public ICollection<TaskItems> Items { get; set; }
        public ICollection<TaskDmDb> Tasks { get; set; }
       
        public string Element { get; set; }
    }
}
