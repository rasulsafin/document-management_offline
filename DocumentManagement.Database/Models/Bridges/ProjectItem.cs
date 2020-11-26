namespace MRS.DocumentManagement.Database.Models
{
    public class ProjectItem
    {
        public int ProjectID { get; set; }
        public Project Project { get; set; }

        public int ItemID { get; set; }
        public Item Item { get; set; }
    }
}
