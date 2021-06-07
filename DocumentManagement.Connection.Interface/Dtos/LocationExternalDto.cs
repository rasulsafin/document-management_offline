namespace MRS.DocumentManagement.Interface.Dtos
{
    public class LocationExternalDto
    {
        public (float x, float y, float z) Location { get; set; }

        public (float x, float y, float z) CameraPosition { get; set; }

        public string Guid { get; set; }

        public ItemExternalDto Item { get; set; }
    }
}
