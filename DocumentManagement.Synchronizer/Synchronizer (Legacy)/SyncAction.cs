namespace MRS.DocumentManagement.Synchronizer.Legacy
{
    public class SyncAction
    {
        public string Synchronizer { get; set; }

        public SyncActionType TypeAction { get; set; }

        public int ID { get; set; }

        public bool SpecialSynchronization { get; set; }

        public object Data { get; set; }

        public bool IsComplete { get; internal set; }
    }
}
