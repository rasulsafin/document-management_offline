namespace MRS.DocumentManagement.Database.Models
{
    public class Location
    {
        [ForbidMergeAttribute]
        public int ID { get; set; }

        public string Position { get; set; }

        public string CameraPosition { get; set; }
    }
}
