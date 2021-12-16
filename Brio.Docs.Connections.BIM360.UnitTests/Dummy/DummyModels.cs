using Brio.Docs.Connections.Bim360.Forge;
using Brio.Docs.Connections.Bim360.Forge.Models;
using System;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;

namespace Brio.Docs.Connections.Bim360.UnitTests.Dummy
{
    internal static class DummyModels
    {
        public static Comment Comment
            => new ()
            {
                ID = DummyStrings.COMMENT_ID,
                Attributes = new Comment.CommentAttributes
                {
                    CreatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
                    SyncedAt = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                    UpdatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                    IssueId = DummyStrings.ISSUE_ID,
                    Body = "The body of the comment",
                    CreatedBy = DummyStrings.USER_ID,
                },
            };

        public static Hub Hub
            => new ()
            {
                ID = DummyStrings.HUB_ID,
                Attributes = new Hub.HubAttributes
                {
                    Name = "Dummy hub",
                    Region = Region.US,
                },
            };

        public static Project Project
            => new ()
            {
                ID = DummyStrings.PROJECT_ID,
                Attributes = new Project.ProjectAttributes
                {
                    Name = "Dummy project",
                },
                Relationships = new Project.ProjectRelationships
                {
                    Hub = new DataContainer<ObjectInfo>(
                        new ObjectInfo
                        {
                            ID = DummyStrings.HUB_ID,
                            Type = Constants.HUB_TYPE,
                        }),
                    RootFolder = new DataContainer<ObjectInfo>(
                        new ObjectInfo
                        {
                            ID = DummyStrings.FOLDER_ID,
                            Type = Constants.FOLDER_TYPE,
                        }),
                    IssuesContainer = new DataContainer<ObjectInfo>(
                        new ObjectInfo
                        {
                            ID = DummyStrings.ISSUE_CONTAINER_ID,
                            Type = Constants.ISSUE_CONTAINER_TYPE,
                        }),
                    LocationContainer = new DataContainer<ObjectInfo>(
                        new ObjectInfo
                        {
                            ID = DummyStrings.LOCATION_CONTAINER_ID,
                            Type = Constants.LOCATION_CONTAINER_TYPE,
                        }),
                },
            };

        public static Issue Issue
            => new ()
            {
                ID = DummyStrings.ISSUE_ID,
                Attributes = new Issue.IssueAttributes
                {
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    SyncedAt = DateTime.UtcNow.AddHours(-2),
                    UpdatedAt = DateTime.UtcNow.AddHours(-3),
                    CloseVersion = 5,
                    ClosedAt = DateTime.UtcNow.AddHours(-2),
                    ClosedBy = DummyStrings.USER_ID,
                    CreatedBy = DummyStrings.USER_ID,
                    StartingVersion = 4,
                    Title = "Dummy issue",
                    Description = "Dummy issue description",
                    LocationDescription = "Dummy issue location description",
                    TargetUrn = DummyStrings.ITEM_ID,
                    DueDate = DateTime.UtcNow.AddDays(2),
                    Identifier = 3455,
                    Status = Status.Open,
                    AssignedTo = DummyStrings.USER_ID,
                    AssignedToType = AssignToType.User,
                    Answer = "Dummy issue answer",
                    AnsweredAt = DummyStrings.USER_ID,
                    AnsweredBy = null,
                    PushpinAttributes = null,
                    Owner = DummyStrings.USER_ID,
                    RootCauseID = null,
                    RootCause = null,
                    QualityUrns = null,
                    PermittedStatuses = new Status[] { },
                    PermittedAttributes = new string[] { },
                    CommentCount = null,
                    AttachmentCount = null,
                    PermittedActions = new string[] { },
                    SheetMetadata = null,
                    LbsLocation = null,
                    NgIssueSubtypeID = null,
                    NgIssueTypeID = null,
                    CustomAttributes = new object[] { },
                    Trades = null,
                    CommentsAttributes = null,
                    AttachmentsAttributes = null,
                },
            };
    }
}
