using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Synchronizator;

namespace DocumentManagement.Connection.Tests
{

    [TestClass]
    public class SyncHelperAnalysisTest
    {
        [TestMethod]
        public void AnalysisDownloadTest()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetUser(1).Rev = 30;

            RevisionCollection local = new RevisionCollection();
            local.GetUser(2).Rev = 10;

            List<SyncAction> actual = SyncHelper.Analysis(local, remote, new UserSyncro());
            actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            List<SyncAction> expected = new List<SyncAction>()
            {
                new SyncAction()
                {
                    Synchronizer = nameof(UserSyncro),
                    TypeAction = TypeSyncAction.Download,
                    ID = 1,
                },
                new SyncAction()
                {
                    Synchronizer = nameof(UserSyncro),
                    TypeAction = TypeSyncAction.Upload,
                    ID = 2,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public void AnalysisNoneActionTest()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetUser(1).Rev = 10;
            remote.GetUser(2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetUser(1).Rev = 10;
            local.GetUser(2).Rev = 10;

            List<SyncAction> actual = SyncHelper.Analysis(local, remote, new UserSyncro());

            List<SyncAction> expected = new List<SyncAction>(){
                new SyncAction()
                {
                    Synchronizer = nameof(UserSyncro),
                    TypeAction = TypeSyncAction.None,
                    ID = 1,
                },
                new SyncAction()
                {
                    Synchronizer = nameof(UserSyncro),
                    TypeAction = TypeSyncAction.None,
                    ID = 2,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public void AnalysisDeleteTest()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetUser(1).Rev = 30;
            remote.GetUser(2).Delete();

            RevisionCollection local = new RevisionCollection();
            local.GetUser(2).Rev = 10;

            List<SyncAction> actual = SyncHelper.Analysis(local, remote, new UserSyncro());
            actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            List<SyncAction> expected = new List<SyncAction>()
            {
                new SyncAction()
                {
                    Synchronizer = nameof(UserSyncro),
                    TypeAction = TypeSyncAction.Download,
                    ID = 1,
                },
                new SyncAction()
                {
                    Synchronizer = nameof(UserSyncro),
                    TypeAction = TypeSyncAction.DeleteLocal,
                    ID = 2,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public void AnalysisUploadTest()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetUser(1).Rev = 30;
            remote.GetUser(2).Rev = 1;

            RevisionCollection local = new RevisionCollection();
            local.GetUser(1).Rev = 100;
            local.GetUser(2).Rev = 100;

            List<SyncAction> actual = SyncHelper.Analysis(local, remote, new UserSyncro());
            actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            List<SyncAction> expected = new List<SyncAction>()
            {
                new SyncAction()
                {
                    Synchronizer = nameof(UserSyncro),
                    TypeAction = TypeSyncAction.Upload,
                    ID = 1,
                },
                new SyncAction()
                {
                    Synchronizer = nameof(UserSyncro),
                    TypeAction = TypeSyncAction.Upload,
                    ID = 2,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public void AnalysisSubSyncTest()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetUser(1).Rev = 1;
            remote.GetUser(2).Rev = 1;

            RevisionCollection local = new RevisionCollection();
            local.GetUser(1).Rev = 1;
            local.GetUser(2).Rev = 1;

            List<SyncAction> actual = SyncHelper.Analysis(local, remote, new SubSyncSyncro());
            actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            List<SyncAction> expected = new List<SyncAction>()
            {
                new SyncAction()
                {
                    Synchronizer = nameof(SubSyncSyncro),
                    TypeAction = TypeSyncAction.Special,
                    ID = 1,
                    SpecialSynchronization = true,
                },
                new SyncAction()
                {
                    Synchronizer = nameof(SubSyncSyncro),
                    TypeAction = TypeSyncAction.Special,
                    ID = 2,
                    SpecialSynchronization = true,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        internal class SubSyncSyncro : ISynchroTable
        {
            Task ISynchroTable.DeleteLocal(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            Task ISynchroTable.DeleteRemote(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            Task ISynchroTable.Download(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            List<Revision> ISynchroTable.GetRevisions(RevisionCollection revisions)
            {
                return revisions.Users;
            }

            Task<List<ISynchroTable>> ISynchroTable.GetSubSynchroList(SyncAction action)
            {
                throw new System.NotImplementedException();
            }


            void ISynchroTable.SetRevision(RevisionCollection revisions, Revision rev)
            {
                throw new System.NotImplementedException();
            }

            Task ISynchroTable.Special(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            SyncAction ISynchroTable.SpecialSynchronization(SyncAction action)
            {
                action.SpecialSynchronization = true;
                action.TypeAction = TypeSyncAction.Special;
                return action;
            }

            Task ISynchroTable.Upload(SyncAction action)
            {
                throw new System.NotImplementedException();
            }
        }

        internal class UserSyncro : ISynchroTable
        {
            public Task DeleteLocal(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            public Task DeleteRemote(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            public Task Download(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            public Task Special(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            public Task Upload(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            List<Revision> ISynchroTable.GetRevisions(RevisionCollection revisions)
            {
                return revisions.Users;
            }

            Task<List<ISynchroTable>> ISynchroTable.GetSubSynchroList(SyncAction action)
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
