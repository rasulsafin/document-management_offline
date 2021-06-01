using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.Interface.Dtos;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class Bim360SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddBim360Synchronization(this IServiceCollection services)
        {
            services.AddScopedFactory<Bim360ConnectionContext>();
            services.AddScoped<Bim360ObjectivesSynchronizer>();
            services.AddScoped<Bim360ProjectsSynchronizer>();
            services.AddScoped<FoldersSyncHelper>();
            services.AddScoped<HubsHelper>();
            services.AddScoped<ItemsSyncHelper>();
            services.AddScoped<ProjectsHelper>();
            services.AddConverter<Issue, ObjectiveExternalDto, IssueObjectiveConverter>();
            services.AddConverter<ObjectiveExternalDto, Issue, ObjectiveIssueConverter>();
            services.AddConverter<Status, ObjectiveStatus, StatusObjectiveStatusConverter>();
            services.AddConverter<ObjectiveStatus, Status, ObjectiveStatusStatusConverter>();
            services.AddConverter<IssueType, DynamicFieldExternalDto, IssueTypeDynamicFieldConverter>();
            services.AddConverter<IssueSnapshot, ObjectiveExternalDto, IssueSnapshotObjectiveConverter>();
            return services;
        }

        private static IServiceCollection AddConverter<TFrom, TTo, TConverter>(this IServiceCollection services)
            where TConverter : class, IConverter<TFrom, TTo>
        {
            services.AddScoped<IConverter<TFrom, TTo>, TConverter>();
            services.AddScoped<ConverterAsync<TFrom, TTo>>(x => x.GetService<IConverter<TFrom, TTo>>() !.Convert);
            return services;
        }
    }
}
