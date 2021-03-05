using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;

namespace MRS.DocumentManagement.Connection.BIM360.Tests.IntegrationTests
{
    [TestClass]
    public class Bim360ObjectivesSynchronizerTests
    {
        public static Bim360ObjectivesSynchronizer synchronizer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext unused)
        {
            var context = new Bim360ConnectionContext();
            synchronizer = new Bim360ObjectivesSynchronizer(context);
        }
    }
}
