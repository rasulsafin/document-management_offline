using MRS.DocumentManagement.Database;

namespace MRS.DocumentManagement.Synchronization.Models
{
    internal class SynchronizingTuple<T> : ISynchronizationChanges
            where T : ISynchronizable<T>
    {
        private T remote;

        public SynchronizingTuple(
                string externalID = null,
                T synchronized = default,
                T local = default,
                T remote = default)
        {
            ExternalID = externalID;
            Synchronized = synchronized;
            Local = local;
            Remote = remote;

            UpdateExternalID();
        }

        public string ExternalID { get; set; }

        public T Synchronized { get; set; }

        public T Local { get; set; }

        public T Remote
        {
            get => remote;
            set
            {
                remote = value;
                UpdateExternalID();
            }
        }

        public bool LocalChanged { get; set; }

        public bool SynchronizedChanged { get; set; }

        public bool RemoteChanged { get; set; }

        public bool HasExternalID => !string.IsNullOrEmpty(ExternalID);

        private void UpdateExternalID()
            => ExternalID ??= Synchronized?.ExternalID ?? Local?.ExternalID ?? Remote?.ExternalID;
    }
}
