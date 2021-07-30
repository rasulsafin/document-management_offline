﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Synchronizer;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class SyncHelperRunActionTest
    {
        [TestMethod]
        public async Task RunAction_NeedDownload_CallDownloadMethod()
        {
            // N | Rem  |  loc
            // 1 |  30  |    0
            // 2 |  10  |   10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 30;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = SyncActionType.Download,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Users, 1).Rev = 30;
            expected.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            Assert.IsFalse(synchro.RunDeleteLocal);
            Assert.IsFalse(synchro.RunDeleteRemote);
            Assert.IsTrue(synchro.RunDownload);
            Assert.IsFalse(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task RunAction_NeedUpload_CallUploadMethod()
        {
            // N | Rem  |  loc
            // 1 |   0  |   30
            // 2 |  10  |   10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 1).Rev = 30;
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = SyncActionType.Upload,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Users, 1).Rev = 30;
            expected.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            Assert.IsFalse(synchro.RunDeleteLocal);
            Assert.IsFalse(synchro.RunDeleteRemote);
            Assert.IsFalse(synchro.RunDownload);
            Assert.IsTrue(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task RunAction_NeedDeleteLocal_CallDeleteLocalMethod()
        {
            // N | Rem  |  loc
            // 1 |  10  |  del
            // 2 |  10  |  10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 10;
            remote.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 1).Delete();
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = SyncActionType.DeleteLocal,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Users, 1).Delete();
            expected.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            Assert.IsTrue(synchro.RunDeleteLocal);
            Assert.IsFalse(synchro.RunDeleteRemote);
            Assert.IsFalse(synchro.RunDownload);
            Assert.IsFalse(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task RunAction_NeedDeleteRemote_CallDeleteRemoteMethod()
        {
            // N | Rem  |  loc
            // 1 | del  |  10
            // 2 |  10  |  10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Delete();
            remote.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 1).Rev = 10;
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = SyncActionType.DeleteRemote,
                ID = 1,
            };

            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Users, 1).Delete();
            expected.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            Assert.IsFalse(synchro.RunDeleteLocal);
            Assert.IsTrue(synchro.RunDeleteRemote);
            Assert.IsFalse(synchro.RunDownload);
            Assert.IsFalse(synchro.RunUpload);
            Assert.IsFalse(synchro.RunSpecial);
            AssertHelper.EqualRevisionCollection(expected, local);
        }

        [TestMethod]
        public async Task RunAction_NoneAction_NotCall()
        {
            // N | Rem  |  loc
            // 1 |  10  |  10
            // 2 |  10  |  10
            RevisionCollection remote = new RevisionCollection();
            remote.GetRevision(NameTypeRevision.Users, 1).Rev = 10;
            remote.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            RevisionCollection local = new RevisionCollection();
            local.GetRevision(NameTypeRevision.Users, 1).Rev = 10;
            local.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

            SyncAction action = new SyncAction()
            {
                Synchronizer = nameof(UserSyncro),
                TypeAction = SyncActionType.None,
                ID = 1,
            };
            var synchro = new UserSyncro();
            await SyncHelper.RunAction(action, synchro, local, remote);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Users, 1).Rev = 10;
            expected.GetRevision(NameTypeRevision.Users, 2).Rev = 10;

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

            public void CheckDBRevision(RevisionCollection local)
            {
                throw new NotImplementedException();
            }

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
                return revisions.GetRevisions(NameTypeRevision.Users);
            }

            void ISynchroTable.SetRevision(RevisionCollection revisions, Revision rev)
            {
                revisions.GetRevision(NameTypeRevision.Users, rev.ID).Rev = rev.Rev;
            }
        }
    }
}