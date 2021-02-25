namespace MRS.DocumentManagement.Synchronizer.Models
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
