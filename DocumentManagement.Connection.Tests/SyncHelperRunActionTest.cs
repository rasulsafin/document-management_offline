using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Synchronizator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class SyncHelperRunActionTest
    {
        [TestMethod]
        public void AnalysisRunDownloadTest()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetUser(1).Rev = 30;

            RevisionCollection local = new RevisionCollection();
            local.GetUser(2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = TypeSyncAction.Download,
                ID = 1,
            };

            var synchro = new UserSyncro();
            SyncHelper.RunActionAsync(action, synchro, local, remote);

            local.Users.Sort((x, y) => x.ID.CompareTo(y.ID));
            RevisionCollection expected = new RevisionCollection();
            local.GetUser(1).Rev = 30;
            local.GetUser(2).Rev = 10;

            AssertHelper.EqualRevisionCollection(expected, local);
        }

        internal class UserSyncro : ISynchroTable
        {

            List<Revision> ISynchroTable.GetRevisions(RevisionCollection revisions)
            {
                return revisions.Users;
            }

            Task<List<ISynchroTable>> ISynchroTable.GetSubSynchroList(int id)
            {
                throw new System.NotImplementedException();
            }

            void ISynchroTable.SetRevision(RevisionCollection revisions, Revision rev)
            {
                throw new System.NotImplementedException();
            }

            SyncAction ISynchroTable.SpecialSynchronization(SyncAction action)
            {
                return action;
            }
        }
    }

}
