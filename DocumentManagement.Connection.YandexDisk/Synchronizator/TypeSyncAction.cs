namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public enum TypeSyncAction
    {
        None,
        Download,
        Upload,
        DeleteLocal,
        DeleteRemote,
        Special,
    }

    public class SyncAction
    {
        public string Synchronizer { get; set; }

        public TypeSyncAction TypeAction { get; set; }

        public int ID { get; set; }

        public bool SpecialSynchronization { get; set; }

        public object Data { get; set; }

    }
}
