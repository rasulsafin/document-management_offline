namespace DocumentManagement.Models.Database
{
    public class ProjectUsers
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public UserDb User { get; set; }
        public ProjectDb Project { get; set; }

    }
}
