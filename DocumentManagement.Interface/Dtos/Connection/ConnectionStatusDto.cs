namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ConnectionStatusDto
    {
        public RemoteConnectionStatusDto Status { get; set; }

        public string Message { get; set; }
    }
}
