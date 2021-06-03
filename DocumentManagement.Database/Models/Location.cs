﻿namespace MRS.DocumentManagement.Database.Models
{
    [MergeContract]
    public class Location
    {
        [ForbidMerge]
        public int ID { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float PositionZ { get; set; }

        public float CameraPositionX { get; set; }

        public float CameraPositionY { get; set; }

        public float CameraPositionZ { get; set; }

        public string BimElementID { get; set; }

        public string ModelName { get; set; }
    }
}
