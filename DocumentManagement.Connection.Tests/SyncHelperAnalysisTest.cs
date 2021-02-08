using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface.SyncData;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class SyncHelperAnalysisTest
    {
        [TestMethod]
        public async Task Analysis_NeedDownload_AddActionDownload()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 30;
            remote.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            List<SyncAction> actual = await SyncHelper.Analysis(local, remote, new TestSyncro());

            actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            List<SyncAction> expected = new List<SyncAction>()
            {
                new SyncAction()
                {
                    Synchronizer = nameof(TestSyncro),
                    TypeAction = SyncActionType.Download,
                    ID = 1,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public async Task Analysis_NeedUpload_AddActionUpload()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 1).Rev = 10;
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 30;

            List<SyncAction> actual = await SyncHelper.Analysis(local, remote, new TestSyncro());

            actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            List<SyncAction> expected = new List<SyncAction>()
            {
                new SyncAction()
                {
                    Synchronizer = nameof(TestSyncro),
                    TypeAction = SyncActionType.Upload,
                    ID = 2,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public async Task Analysis_NotChenge_NoneAction()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 10;
            remote.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 1).Rev = 10;
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            List<SyncAction> actual = await SyncHelper.Analysis(local, remote, new TestSyncro());

            List<SyncAction> expected = new List<SyncAction>();

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public async Task Analysis_NeedDownloadMore_AddActionDownloadMore()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 30;
            remote.GetRevision(NameTypeRevision.Users, 2).Delete();

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            List<SyncAction> actual = await SyncHelper.Analysis(local, remote, new TestSyncro());
            actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            List<SyncAction> expected = new List<SyncAction>()
            {
                new SyncAction()
                {
                    Synchronizer = nameof(TestSyncro),
                    TypeAction = SyncActionType.Download,
                    ID = 1,
                },
                new SyncAction()
                {
                    Synchronizer = nameof(TestSyncro),
                    TypeAction = SyncActionType.DeleteLocal,
                    ID = 2,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public async Task Analysis_NeedUploadMore_AddActionUploadMore()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 30;
            remote.GetRevision(NameTypeRevision.Users, 2).Rev = 1;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 1).Rev = 100;
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 100;

            List<SyncAction> actual = await SyncHelper.Analysis(local, remote, new TestSyncro());
            actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            List<SyncAction> expected = new List<SyncAction>()
            {
                new SyncAction()
                {
                    Synchronizer = nameof(TestSyncro),
                    TypeAction = SyncActionType.Upload,
                    ID = 1,
                },
                new SyncAction()
                {
                    Synchronizer = nameof(TestSyncro),
                    TypeAction = SyncActionType.Upload,
                    ID = 2,
                },
            };

            AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }

        [TestMethod]
        public async Task Analysis_NoChenge_NoChangeCollect()
        {
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 1;
            remote.GetRevision(NameTypeRevision.Users, 2).Rev = 1;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 1).Rev = 1;
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 1;

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Users, 1).Rev = 1;
            expected.GetRevision(NameTypeRevision.Users, 2).Rev = 1;

            List<SyncAction> syncActions = await SyncHelper.Analysis(local, remote, new SubSyncSyncro());

            AssertHelper.EqualRevisionCollection(expected, local);
            AssertHelper.EqualRevisionCollection(expected, remote);
        }

        internal class SubSyncSyncro : ISynchroTable
        {
            public void CheckDBRevision(RevisionCollection local)
            {
                throw new System.NotImplementedException();
            }

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
                return revisions.GetRevisions(NameTypeRevision.Users);
            }

            void ISynchroTable.SetRevision(RevisionCollection revisions, Revision rev)
            {
                throw new System.NotImplementedException();
            }

            Task ISynchroTable.Upload(SyncAction action)
            {
                throw new System.NotImplementedException();
            }
        }

        internal class TestSyncro : ISynchroTable
        {
            public void CheckDBRevision(RevisionCollection local)
            {
                throw new System.NotImplementedException();
            }

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
                return revisions.GetRevisions(NameTypeRevision.Users);
            }

            void ISynchroTable.SetRevision(RevisionCollection revisions, Revision rev)
            {
                throw new System.NotImplementedException();
            }

        }
    }
}
