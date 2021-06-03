namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    public abstract class ASnapshotEntity<T>
    {
        protected ASnapshotEntity(T entity)
            => Entity = entity;

        public T Entity { get; set; }

        public abstract string ID { get; }
    }
}
