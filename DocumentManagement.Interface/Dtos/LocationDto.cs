namespace MRS.DocumentManagement.Interface.Dtos
{
    public class LocationDto
    {
        public int ID { get; set; }

        public (float x, float y, float z) Position { get; set; }

        public (float x, float y, float z) CameraPosition { get; set; }

        public string BimElementID { get; set; }

        public string ModelName { get; set; }
    }
}
