using System;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.UnitTests
{
    public static class MockData
    {
        public static Comment Commment
            => new ()
            {
                ID = $"comment-{Guid.NewGuid()}",
                Attributes = new Comment.CommentAttributes
                {
                    CreatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
                    SyncedAt = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                    UpdatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                    IssueId = $"issue-{Guid.NewGuid()}",
                    Body = "The body of the comment",
                    CreatedBy = $"user-{Guid.NewGuid()}",
                },
            };
    }
}
