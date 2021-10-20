namespace MRS.DocumentManagement.Interface.Dtos
{
    public class LocationDto
    {
        public int ID { get; set; }

        public (float x, float y, float z) Position { get; set; }

        public (float x, float y, float z) CameraPosition { get; set; }

        public string Guid { get; set; }

        public ItemDto Item { get; set; }
    }
}
