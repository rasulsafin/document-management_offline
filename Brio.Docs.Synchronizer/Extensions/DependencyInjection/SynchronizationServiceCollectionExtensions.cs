using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Mergers;
using Brio.Docs.Synchronization.Mergers.ChildrenMergers;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Strategies;
using Brio.Docs.Synchronization.Utilities.Finders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<Synchronizer>();
            services.AddScoped<ISynchronizationStrategy<Project, ProjectExternalDto>, ProjectStrategy>();
            services.AddScoped<ISynchronizationStrategy<Objective, ObjectiveExternalDto>, ObjectiveStrategy>();

            services.AddScoped<IMerger<Project>, ProjectMerger>();
            services.AddScoped<IMerger<Objective>, ObjectiveMerger>();
            services.AddScoped<IMerger<Item>, ItemMerger>();
            services.AddScoped<IMerger<DynamicField>, DynamicFieldMerger>();
            services.AddScoped<IMerger<Location>, LocationMerger>();
            services.AddScoped<IMerger<BimElement>, BimElementMerger>();

            services.AddScoped<IAttacher<Item>, ItemAttacher>();

            services.AddScoped<IExternalIdUpdater<Item>, ItemExternalIdUpdater>();
            services.AddScoped<IExternalIdUpdater<DynamicField>, DynamicFieldExternalIdUpdater>();

            services.AddProjectChildrenMergers();
            services.AddObjectiveChildrenMergers();
            services.AddDynamicFieldChildrenMergers();

            return services;
        }

        private static IServiceCollection AddDynamicFieldChildrenMergers(this IServiceCollection services)
            => services
               .AddScoped<IChildrenMerger<DynamicField, DynamicField>, DynamicFieldDynamicFieldsMerger>()
               .AddFactory<IChildrenMerger<DynamicField, DynamicField>>();

        private static IServiceCollection AddObjectiveChildrenMergers(this IServiceCollection services)
        {
            services.AddScoped<IChildrenMerger<Objective, Item>, ObjectiveItemsMerger>();

            services.AddScoped<IChildrenMerger<Objective, DynamicField>, ObjectiveDynamicFieldsMerger>();
            services.AddScoped<IChildrenMerger<Objective, BimElement>, ObjectiveBimElementsMerger>();

            services.AddFactory<IChildrenMerger<Objective, Item>>();
            services.AddFactory<IChildrenMerger<Objective, DynamicField>>();
            services.AddFactory<IChildrenMerger<Objective, BimElement>>();
            return services;
        }

        private static IServiceCollection AddProjectChildrenMergers(this IServiceCollection services)
            => services
               .AddScoped<IChildrenMerger<Project, Item>, ProjectItemsMerger>()
               .AddFactory<IChildrenMerger<Project, Item>>();

        private static bool DoesNeedItem(Item item, SynchronizingTuple<Item> tuple)
            => tuple.DoesNeed(item) ||
                item.RelativePath == (string)tuple.GetPropertyValue(nameof(Item.RelativePath));
    }
}
