using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Services;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class SyncHelperRunActionTest
    {
        [TestMethod]
        public async Task RunDownloadTestAsync()
        {
            // N | Rem  |  loc
            // 1 |  30  |    0
            // 2 |  10  |   10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(TableRevision.Users, 1).Rev = 30;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(TableRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = TypeSyncAction.Download,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Users, 1).Rev = 30;
            expected.GetRevision(TableRevision.Users, 2).Rev = 10;

            Assert.IsFalse(synchro.RunDeleteLocal);
            Assert.IsFalse(synchro.RunDeleteRemote);
            Assert.IsTrue(synchro.RunDownload);
            Assert.IsFalse(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task RunUploadTestAsync()
        {
            // N | Rem  |  loc
            // 1 |   0  |   30
            // 2 |  10  |   10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(TableRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(TableRevision.Users, 1).Rev = 30;
            local.GetRevision(TableRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = TypeSyncAction.Upload,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Users, 1).Rev = 30;
            expected.GetRevision(TableRevision.Users, 2).Rev = 10;

            Assert.IsFalse(synchro.RunDeleteLocal);
            Assert.IsFalse(synchro.RunDeleteRemote);
            Assert.IsFalse(synchro.RunDownload);
            Assert.IsTrue(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task RunDeleteLocalTestAsync()
        {
            // N | Rem  |  loc
            // 1 |  10  |  del
            // 2 |  10  |  10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(TableRevision.Users, 1).Rev = 10;
            remote.GetRevision(TableRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(TableRevision.Users, 1).Delete();
            local.GetRevision(TableRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = TypeSyncAction.DeleteLocal,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            // local.Users.Sort((x, y) => x.ID.CompareTo(y.ID));
            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Users, 1).Delete();
            expected.GetRevision(TableRevision.Users, 2).Rev = 10;

            Assert.IsTrue(synchro.RunDeleteLocal);
            Assert.IsFalse(synchro.RunDeleteRemote);
            Assert.IsFalse(synchro.RunDownload);
            Assert.IsFalse(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task RunDeleteRemoteTestAsync()
        {
            // N | Rem  |  loc
            // 1 | del  |  10
            // 2 |  10  |  10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(TableRevision.Users, 1).Delete();
            remote.GetRevision(TableRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(TableRevision.Users, 1).Rev = 10;
            local.GetRevision(TableRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = TypeSyncAction.DeleteRemote,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            // local.Users.Sort((x, y) => x.ID.CompareTo(y.ID));
            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Users, 1).Delete();
            expected.GetRevision(TableRevision.Users, 2).Rev = 10;

            Assert.IsFalse(synchro.RunDeleteLocal);
            Assert.IsTrue(synchro.RunDeleteRemote);
            Assert.IsFalse(synchro.RunDownload);
            Assert.IsFalse(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task RunSpecialTestAsync()
        {
            // N | Rem  |  loc
            // 1 |  10  |  10  spec
            // 2 |  10  |  10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(TableRevision.Users, 1).Rev = 10;
            remote.GetRevision(TableRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(TableRevision.Users, 1).Rev = 10;
            local.GetRevision(TableRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = TypeSyncAction.Special,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
             {
                 await SyncHelper.RunAction(action, synchro, local, remote);
             });

            // local.Users.Sort((x, y) => x.ID.CompareTo(y.ID));
            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Users, 1).Rev = 10;
            expected.GetRevision(TableRevision.Users, 2).Rev = 10;

            Assert.IsFalse(synchro.RunDeleteLocal);
            Assert.IsFalse(synchro.RunDeleteRemote);
            Assert.IsFalse(synchro.RunDownload);
            Assert.IsFalse(synchro.RunUpload);
            // Assert.IsTrue(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task NoneRunTestAsync()
        {
            // N | Rem  |  loc
            // 1 |  10  |  10
            // 2 |  10  |  10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(TableRevision.Users, 1).Rev = 10;
            remote.GetRevision(TableRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(TableRevision.Users, 1).Rev = 10;
            local.GetRevision(TableRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = TypeSyncAction.None,
                ID = 1,
            };
            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            // local.Users.Sort((x, y) => x.ID.CompareTo(y.ID));
            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Users, 1).Rev = 10;
            expected.GetRevision(TableRevision.Users, 2).Rev = 10;

            Assert.IsFalse(synchro.RunDeleteLocal);
            Assert.IsFalse(synchro.RunDeleteRemote);
            Assert.IsFalse(synchro.RunDownload);
            Assert.IsFalse(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        internal class UserSyncro : ISynchroTable
        {
            public bool RunDownload { get; private set; }

            public bool RunUpload { get; private set; }

            public bool RunDeleteRemote { get; private set; }

            public bool RunDeleteLocal { get; private set; }

            public bool RunSpecial { get; private set; }

            public Task DeleteLocal(SyncAction action)
            {
                RunDeleteLocal = true;
                return Task.CompletedTask;
            }

            public Task DeleteRemote(SyncAction action)
            {
                RunDeleteRemote = true;
                return Task.CompletedTask;
            }

            public Task Download(SyncAction action)
            {
                RunDownload = true;
                return Task.CompletedTask;
            }

            public Task Special(SyncAction action)
            {
                RunSpecial = true;
                return Task.CompletedTask;
            }

            public Task Upload(SyncAction action)
            {
                RunUpload = true;
                return Task.CompletedTask;
            }

            List<Revision> ISynchroTable.GetRevisions(RevisionCollection revisions)
            {
                return revisions.GetRevisions(TableRevision.Users);
            }

            Task<List<ISynchroTable>> ISynchroTable.GetSubSynchroList(SyncAction action)
            {
                throw new System.NotImplementedException();
            }

            void ISynchroTable.SetRevision(RevisionCollection revisions, Revision rev)
            {
                revisions.GetRevision(TableRevision.Users, rev.ID).Rev = rev.Rev;
            }

            SyncAction ISynchroTable.SpecialSynchronization(SyncAction action)
            {
                return action;
            }
        }
    }

}
