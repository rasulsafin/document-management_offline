namespace Brio.Docs.Database.Models
{
    [MergeContract]
    public class Location
    {
        [ForbidMerge]
        public int ID { get; set; }

        public double PositionX { get; set; }

        public double PositionY { get; set; }

        public double PositionZ { get; set; }

        public double CameraPositionX { get; set; }

        public double CameraPositionY { get; set; }

        public double CameraPositionZ { get; set; }

        public string Guid { get; set; }

        [ForbidMerge]
        public int ItemID { get; set; }

        [ForbidMerge]
        public Item Item { get; set; }

        [ForbidMerge]
        public int ObjectiveID { get; set; }

        [ForbidMerge]
        public Objective Objective { get; set; }
    }
}
