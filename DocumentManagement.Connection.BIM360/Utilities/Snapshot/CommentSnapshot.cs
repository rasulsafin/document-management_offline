using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class CommentSnapshot : ASnapshotEntity<Comment>
    {
        public CommentSnapshot(Comment entity)
            : base(entity)
        {
        }

        public string Author { get; set; }

        public override string ID => Entity.ID;
    }
}
