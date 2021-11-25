using System;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization;
using Brio.Docs.Connections.Bim360.Synchronization.Converters;
using Brio.Docs.Connections.Bim360.Synchronization.Factories;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities.Objective;
using Brio.Docs.Connections.Bim360.Synchronizers;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class Bim360SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddBim360Synchronization(this IServiceCollection services)
        {
            services.AddScopedFactory<TokenHelper>();
            services.AddScopedFactory<Authenticator>();
            services.AddScoped<IAccessController>(x => x.GetService<Authenticator>());

            services.AddContext();

            services.AddSynchronizer<Bim360ObjectivesSynchronizer, ObjectiveExternalDto>();
            services.AddSynchronizer<Bim360ProjectsSynchronizer, ProjectExternalDto>();

            services.AddScoped<ItemsSyncHelper>();
            services.AddScoped<MetaCommentHelper>();
            services.AddScoped<ObjectiveGetter>();
            services.AddScoped<ObjectiveUpdater>();
            services.AddScoped<ObjectiveRemover>();

            services.AddConverter<Issue, ObjectiveExternalDto, IssueObjectiveConverter>();
            services.AddConverter<ObjectiveExternalDto, Issue, ObjectiveIssueConverter>();
            services.AddConverter<IssueSnapshot, ObjectiveStatus, IssueSnapshotObjectiveStatusConverter>();
            services.AddConverter<ObjectiveExternalDto, Status, ObjectiveIssueStatusConverter>();
            services.AddConverter<IssueSnapshot, ObjectiveExternalDto, IssueSnapshotObjectiveConverter>();
            return services;
        }

        private static IServiceCollection AddContext(this IServiceCollection services)
        {
            services.AddScoped<Bim360ConnectionContext>();
            services.AddScopedFactory<Bim360ConnectionContext>();
            services.AddScoped<IFactory<ConnectionInfoExternalDto, IConnectionContext>, ContextFactory>();
            return services;
        }

        private static IServiceCollection AddConverter<TFrom, TTo, TConverter>(this IServiceCollection services)
            where TConverter : class, IConverter<TFrom, TTo>
        {
            services.AddScoped<IConverter<TFrom, TTo>, TConverter>();
            return services;
        }

        private static IServiceCollection AddSynchronizer<TSynchronizer, TDto>(this IServiceCollection services)
            where TSynchronizer : class, ISynchronizer<TDto>
        {
            services.AddScoped<TSynchronizer>();
            services.AddScoped<Func<TSynchronizer>>(x => x.GetRequiredService<TSynchronizer>);
            services.AddScoped<IFactory<ISynchronizer<TDto>>, Factory<TSynchronizer>>();
            return services;
        }
    }
}
