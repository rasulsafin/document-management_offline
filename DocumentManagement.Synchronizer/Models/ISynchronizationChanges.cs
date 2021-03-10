namespace MRS.DocumentManagement.Synchronization.Models
{
    internal interface ISynchronizationChanges
    {
        bool LocalChanged { get; set; }

        bool SynchronizedChanged { get; set; }

        bool RemoteChanged { get; set; }
    }
}
