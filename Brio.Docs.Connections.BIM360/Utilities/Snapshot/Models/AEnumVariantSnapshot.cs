namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models
{
    internal abstract class AEnumVariantSnapshot<T> : ASnapshotEntity<T>
    {
        private string id;

        protected AEnumVariantSnapshot(T entity, ProjectSnapshot projectSnapshot)
            : base(entity)
            => ProjectSnapshot = projectSnapshot;

        public ProjectSnapshot ProjectSnapshot { get; }

        public override string ID => id;

        public void SetExternalID(string externalID)
            => id = externalID;
    }
}
