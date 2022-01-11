using System;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Mergers
{
    internal class ProjectMerger : IMerger<Project>
    {
        private readonly Lazy<IChildrenMerger<Project, Item>> itemChildrenMerger;

        public ProjectMerger(IFactory<IChildrenMerger<Project, Item>> itemChildrenMergerFactory)
            => this.itemChildrenMerger = new Lazy<IChildrenMerger<Project, Item>>(itemChildrenMergerFactory.Create);

        public async ValueTask Merge(SynchronizingTuple<Project> tuple)
        {
            tuple.Merge(project => project.Title);
            await itemChildrenMerger.Value.MergeChildren(tuple).ConfigureAwait(false);
        }
    }
}
