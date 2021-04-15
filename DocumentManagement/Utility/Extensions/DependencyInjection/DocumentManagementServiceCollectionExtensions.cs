using System;
using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Services;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Utility.Factories;
using MRS.DocumentManagement.Utility.Mapping.Converters;
using MRS.DocumentManagement.Utility.Mapping.Resolvers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DocumentManagementServiceCollectionExtensions
    {
        public static IServiceCollection AddDocumentManagement(this IServiceCollection services)
        {
            services.AddMappingResolvers();

            services.AddScoped<ItemHelper>();
            services.AddScoped<DynamicFieldHelper>();
            services.AddScoped<ConnectionHelper>();

            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IConnectionService, ConnectionService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IObjectiveService, ObjectiveService>();
            services.AddScoped<IObjectiveTypeService, ObjectiveTypeService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IConnectionTypeService, ConnectionTypeService>();

            services.AddSingleton<IRequestService, RequestQueueService>();
            services.AddSingleton<IRequestQueueService, RequestQueueService>();
            services.AddSingleton<CryptographyHelper>();

            services.AddFactories();
            services.AddExternal();
            return services;
        }

        private static IServiceCollection AddFactories(this IServiceCollection services)
        {
            services.AddScoped<Func<Type, IConnection>>(x => type => (IConnection)x.GetRequiredService(type));
            services.AddScoped<IFactory<Type, IConnection>, Factory<Type, IConnection>>();

            services.AddScoped<Func<IServiceScope, Type, IConnection>>(
                x => (scope, type) => (IConnection)(scope?.ServiceProvider ?? x).GetRequiredService(type));
            services.AddScoped<IFactory<IServiceScope, Type, IConnection>, Factory<IServiceScope, Type, IConnection>>();

            services.AddScoped<IFactory<IServiceScope, SynchronizingData>, SynchronizationDataFactory>();
            services.AddScoped<IFactory<IServiceScope, ConnectionHelper>, ConnectionHelperFactory>();

            services.AddScopedFactory<DMContext>();
            services.AddScopedFactory<IMapper>();
            return services;
        }

        private static IServiceCollection AddExternal(this IServiceCollection services)
            => ConnectionCreator.GetDependencyInjectionMethods()
               .Aggregate(
                    services,
                    (aggregated, method) => (IServiceCollection)method.Invoke(null, new object[] { aggregated }));

        private static IServiceCollection AddMappingResolvers(this IServiceCollection services)
        {
            services.AddTransient<BimElementObjectiveTypeConverter>();

            services.AddTransient<ConnectionTypeAppPropertiesResolver>();
            services.AddTransient<ConnectionTypeDtoAppPropertiesResolver>();
            services.AddTransient<ConnectionInfoAuthFieldValuesResolver>();
            services.AddTransient<ConnectionInfoDtoAuthFieldValuesResolver>();

            services.AddTransient<DynamicFieldDtoToModelValueResolver>();
            services.AddTransient<DynamicFieldExternalToModelValueResolver>();
            services.AddTransient<DynamicFieldModelToDtoValueResolver>();
            services.AddTransient<DynamicFieldModelToExternalValueResolver>();

            services.AddTransient<ObjectiveExternalDtoProjectIdResolver>();
            services.AddTransient<ObjectiveExternalDtoProjectResolver>();
            services.AddTransient<ObjectiveExternalDtoObjectiveTypeResolver>();
            services.AddTransient<ObjectiveExternalDtoObjectiveTypeIdResolver>();
            services.AddTransient<ObjectiveExternalDtoAuthorIdResolver>();
            services.AddTransient<ObjectiveExternalDtoAuthorResolver>();
            services.AddTransient<ObjectiveObjectiveTypeResolver>();
            services.AddTransient<ObjectiveProjectIDResolver>();
            services.AddTransient<BimElementObjectiveTypeConverter>();

            services.AddTransient<ItemFileNameResolver>();
            services.AddTransient<ItemFullPathResolver>();
            services.AddTransient<ItemExternalDtoRelativePathResolver>();
            return services;
        }

        private static IServiceCollection AddScopedFactory<TResult>(this IServiceCollection services)
        {
            services.AddScoped<Func<IServiceScope, TResult>>(
                x => scope => (scope?.ServiceProvider ?? x).GetRequiredService<TResult>());
            services.AddScoped<IFactory<IServiceScope, TResult>, Factory<IServiceScope, TResult>>();
            return services;
        }
    }
}
