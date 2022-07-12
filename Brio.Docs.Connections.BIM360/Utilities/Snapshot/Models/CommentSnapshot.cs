using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models
{
    internal class CommentSnapshot : ASnapshotEntity<Comment>
    {
        public CommentSnapshot(Comment entity)
            : base(entity)
        {
        }

        public string Author { get; init; }

        public override string ID => Entity.ID;
    }
}
