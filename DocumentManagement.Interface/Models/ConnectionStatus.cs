namespace DocumentManagement.Interface.Models
{
    public struct ConnectionStatus
    {
        public RemoteConnectionStatus Status { get; set; }
        public string Message { get; set; }
    }
}
