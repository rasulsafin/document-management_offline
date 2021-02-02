namespace MRS.DocumentManagement.Database.Models
{
    public class BimElementObjective
    {
        public int ObjectiveID { get; set; }

        public Objective Objective { get; set; }

        public int BimElementID { get; set; }

        public BimElement BimElement { get; set; }
    }
}
