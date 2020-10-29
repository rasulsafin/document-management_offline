namespace DocumentManagement.Models.Database
{
    public class ProjectItems
    {
        public int ProjectId { get; set; }
        public int ItemId { get; set; }
        public ItemDb Item { get; set; }
        public ProjectDb Project { get; set; }
    }
}
