using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Synchronization.Models;

namespace Brio.Docs.Connections.Bim360.UnitTests.Dummy
{
    internal class DummySynchronizationModels
    {
        public static LinkedInfo LinkedInfo
            => new ()
            {
                Urn = DummyStrings.ITEM_ID,
                Version = 0,
                Offset = Vector3d.One,
            };
    }
}
