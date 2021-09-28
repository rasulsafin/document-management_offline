using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Models;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class IfcConfigUtilities
    {
        private readonly Downloader downloader;

        public IfcConfigUtilities(Downloader downloader)
            => this.downloader = downloader;

        public static StatusesRelations GetDefaultStatusesConfig()
        {
            var conf = new StatusesRelations
            {
                Get = new[]
                {
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.Draft,
                        Destination = ObjectiveStatus.Open,
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.Open,
                        Destination = ObjectiveStatus.Late,
                        Conditions = new[]
                        {
                            new RelationCondition
                            {
                                PropertyName = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.DueDate),
                                ComparisonType = RelationComparisonType.Greater,
                                ValueType = RelationComparisonValueType.DateTime,
                                Values = new object[] { DateTimeValues.Now },
                            },
                        },
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.Open,
                        Destination = ObjectiveStatus.Open,
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.NotApproved,
                        Destination = ObjectiveStatus.Late,
                        Conditions = new[]
                        {
                            new RelationCondition
                            {
                                PropertyName = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.DueDate),
                                ComparisonType = RelationComparisonType.Greater,
                                ValueType = RelationComparisonValueType.DateTime,
                                Values = new object[] { DateTimeValues.Now },
                            },
                        },
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.NotApproved,
                        Destination = ObjectiveStatus.Open,
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.Answered,
                        Destination = ObjectiveStatus.InProgress,
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.WorkCompleted,
                        Destination = ObjectiveStatus.InProgress,
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.ReadyToInspect,
                        Destination = ObjectiveStatus.InProgress,
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.InDispute,
                        Destination = ObjectiveStatus.InProgress,
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.InDispute,
                        Destination = ObjectiveStatus.InProgress,
                    },
                    new RelationRule<Status, ObjectiveStatus>
                    {
                        Source = Status.Closed,
                        Destination = ObjectiveStatus.Ready,
                    },
                },
                Set = new[]
                {
                    new RelationRule<ObjectiveStatus, Status>
                    {
                        Source = ObjectiveStatus.Open,
                        Destination = Status.Open,
                        Conditions = new[]
                        {
                            new RelationCondition
                            {
                                ObjectType = ComparisonObjectType.Bim360,
                                PropertyName = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.Status),
                                ComparisonType = RelationComparisonType.NotEqual,
                                ValueType = RelationComparisonValueType.String,
                                Values = new object[] { Status.NotApproved },
                            },
                        },
                    },
                    new RelationRule<ObjectiveStatus, Status>
                    {
                        Source = ObjectiveStatus.Late,
                        Destination = Status.NotApproved,
                        Conditions = new[]
                        {
                            new RelationCondition
                            {
                                PropertyName = nameof(ObjectiveExternalDto.DueDate),
                                ObjectType = ComparisonObjectType.BrioMrs,
                                ComparisonType = RelationComparisonType.Less,
                                ValueType = RelationComparisonValueType.DateTime,
                                Values = new object[] { DateTimeValues.Now },
                            },
                        },
                    },
                    new RelationRule<ObjectiveStatus, Status>
                    {
                        Source = ObjectiveStatus.Late,
                        Destination = Status.Open,
                        Conditions = new[]
                        {
                            new RelationCondition
                            {
                                PropertyName = nameof(ObjectiveExternalDto.DueDate),
                                ObjectType = ComparisonObjectType.BrioMrs,
                                ComparisonType = RelationComparisonType.Less,
                                ValueType = RelationComparisonValueType.DateTime,
                                Values = new object[] { DateTimeValues.Now },
                            },
                        },
                    },
                    new RelationRule<ObjectiveStatus, Status>
                    {
                        Source = ObjectiveStatus.InProgress,
                        Destination = Status.InDispute,
                        Conditions = new[]
                        {
                            new RelationCondition
                            {
                                ObjectType = ComparisonObjectType.Bim360,
                                PropertyName = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.Status),
                                ComparisonType = RelationComparisonType.NotEqual,
                                ValueType = RelationComparisonValueType.String,
                                Values = new object[] { Status.Answered, Status.WorkCompleted, Status.ReadyToInspect },
                            },
                        },
                    },
                    new RelationRule<ObjectiveStatus, Status>
                    {
                        Source = ObjectiveStatus.Ready,
                        Destination = Status.Closed,
                    },
                    new RelationRule<ObjectiveStatus, Status>
                    {
                        Source = ObjectiveStatus.Undefined,
                        Destination = Status.Draft,
                    },
                    new RelationRule<ObjectiveStatus, Status>
                    {
                        Source = ObjectiveStatus.Undefined,
                        Destination = Status.Open,
                    },
                },
                Priority = new[]
                {
                    Status.NotApproved,
                    Status.ReadyToInspect,
                    Status.WorkCompleted,
                    Status.InDispute,
                    Status.Answered,
                    Status.Open,
                    Status.Closed,
                    Status.Draft,
                },
            };

            return conf;
        }

        public async Task<IfcConfig> GetConfig(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            ItemSnapshot itemSnapshot)
        {
            if (string.IsNullOrWhiteSpace(obj.Location?.Item?.FileName))
                return null;

            var configName = obj.Location.Item.FileName + MrsConstants.CONFIG_EXTENSION;
            var config = project.Items
               .Where(
                    x => x.Value.Entity.Relationships.Parent.Data.ID ==
                        itemSnapshot.Entity.Relationships.Parent.Data.ID)
               .FirstOrDefault(
                    x => string.Equals(
                        x.Value.Entity.Attributes.DisplayName,
                        configName,
                        StringComparison.OrdinalIgnoreCase))
               .Value;

            return await GetConfigFromRemote<IfcConfig>(project, config);
        }

        public async Task<StatusesRelations> GetStatusesConfig(ProjectSnapshot project)
        {
            var configName = MrsConstants.STATUSES_CONFIG_NAME + MrsConstants.CONFIG_EXTENSION;
            var config = project.Items
               .FirstOrDefault(
                    x => string.Equals(
                        x.Value.Entity.Attributes.DisplayName,
                        configName,
                        StringComparison.OrdinalIgnoreCase))
               .Value;

            return await GetConfigFromRemote<StatusesRelations>(project, config);
        }

        private async Task<T> GetConfigFromRemote<T>(ProjectSnapshot project, ItemSnapshot config)
            where T : class
        {
            if (config != null)
            {
                var downloadedConfig = await downloader.Download(
                    project.ID,
                    config.Entity,
                    config.Version,
                    Path.GetTempFileName());

                if (downloadedConfig?.Exists ?? false)
                {
                    var ifcConfig = JsonConvert.DeserializeObject<T>(
                        await File.ReadAllTextAsync(downloadedConfig.FullName));
                    downloadedConfig.Delete();
                    return ifcConfig;
                }
            }

            return null;
        }
    }
}
