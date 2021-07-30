namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal abstract class AEnumVariantSnapshot<T> : ASnapshotEntity<T>
    {
        private string id;

        protected AEnumVariantSnapshot(T entity, ProjectSnapshot projectSnapshot)
            : base(entity)
        {
            ProjectSnapshot = projectSnapshot;
        }

        public ProjectSnapshot ProjectSnapshot { get; }

        public override string ID
        {
            get => id;
        }

        public void SetExternalID(string externalID)
            => id = externalID;
    }
}
