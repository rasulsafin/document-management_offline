namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models
{
    internal abstract class ASnapshotEntity<T>
    {
        protected ASnapshotEntity(T entity)
            => Entity = entity;

        public T Entity { get; set; }

        public abstract string ID { get; }
    }
}
