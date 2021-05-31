namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    public class ASnapshotEntity<T>
    {
        public ASnapshotEntity(T entity)
            => Entity = entity;

        public T Entity { get; set; }
    }
}
