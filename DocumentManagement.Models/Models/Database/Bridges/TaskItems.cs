namespace DocumentManagement.Models.Database
{
    public class TaskItems
    {
        public int TaskId { get; set; }
        public int ItemId { get; set; }
        public TaskDmDb Task { get; set; }
        public ItemDb Item { get; set; }
    }
}
