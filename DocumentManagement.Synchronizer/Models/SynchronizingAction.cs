namespace MRS.DocumentManagement.Synchronization.Models
{
    public enum SynchronizingAction
    {
        Nothing,
        Merge,
        AddToLocal,
        AddToRemote,
        RemoveFromLocal,
        RemoveFromRemote,
    }
}
