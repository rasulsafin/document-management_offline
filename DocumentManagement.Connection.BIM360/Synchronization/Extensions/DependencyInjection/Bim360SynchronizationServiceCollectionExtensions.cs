using System;
using Brio.Docs;
using Brio.Docs.Connection.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connection.Bim360.Forge.Utils;
using Brio.Docs.Connection.Bim360.Synchronization;
using Brio.Docs.Connection.Bim360.Synchronization.Converters;
using Brio.Docs.Connection.Bim360.Synchronization.Factories;
using Brio.Docs.Connection.Bim360.Synchronization.Utilities;
using Brio.Docs.Connection.Bim360.Synchronizers;
using Brio.Docs.Connection.Bim360.Utilities.Snapshot;
using Brio.Docs.General.Utils.Factories;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class Bim360SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddBim360Synchronization(this IServiceCollection services)
        {
            services.AddScopedFactory<TokenHelper>();
            services.AddScopedFactory<Authenticator>();

            services.AddContext();

            services.AddSynchronizer<Bim360ObjectivesSynchronizer, ObjectiveExternalDto>();
            services.AddSynchronizer<Bim360ProjectsSynchronizer, ProjectExternalDto>();

            services.AddScoped<ItemsSyncHelper>();

            services.AddConverter<Issue, ObjectiveExternalDto, IssueObjectiveConverter>();
            services.AddConverter<ObjectiveExternalDto, Issue, ObjectiveIssueConverter>();
            services.AddConverter<Status, ObjectiveStatus, StatusObjectiveStatusConverter>();
            services.AddConverter<ObjectiveStatus, Status, ObjectiveStatusStatusConverter>();
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
