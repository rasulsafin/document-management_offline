using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;

namespace DocumentManagement.Connection.Tests
{
    public class AssertHelper
    {
        public static void EqualSyncAction(SyncAction expected, SyncAction actual)
        {
            if (NUllComparer(expected, actual)) return;
            Assert.AreEqual(expected.ID, actual.ID, "Не совпали id");
            Assert.AreEqual(expected.Synchronizer, actual.Synchronizer, "Не совпали Synchronizer");
            Assert.AreEqual(expected.TypeAction, actual.TypeAction, "Не совпали TypeAction");
            Assert.AreEqual(expected.SpecialSynchronization, actual.SpecialSynchronization, "Не совпали SubSynchronization");
        }

        public static void EqualList<T>(List<T> expected, List<T> actual, Action<T, T> equalAction)
        {
            if (NUllComparer(expected, actual)) return;
            Assert.AreEqual(expected.Count, actual.Count, "Не совподает число элементов");
            for (int i = 0; i < actual.Count; i++)
            {
                equalAction(expected[i], actual[i]);
            }
        }

        public static void EqualRevisionCollection(RevisionCollection expected, RevisionCollection actual)
        {
            if (NUllComparer(expected, actual)) return;
            EqualList(expected.Users, actual.Users, EqualRevision);
            EqualList(expected.Objectives, actual.Objectives, EqualRevision);
            EqualList(expected.Projects, actual.Projects, EqualRevision);
            EqualList(expected.Items, actual.Items, EqualRevision);
        }

        //public static void EqualRevisionChildsItem(RevisionChildsItem expected, RevisionChildsItem actual)
        //{
        //    if (NUllComparer(expected, actual)) return;
        //    EqualRevision(expected, actual);
        //    EqualList(expected.Items, actual.Items, EqualRevision);
        //}

        public static void EqualRevision(Revision expected, Revision actual)
        {
            if (NUllComparer(expected, actual)) return;
            Assert.AreEqual(expected.ID, actual.ID, "Не совпали id");
            Assert.AreEqual(expected.Rev, actual.Rev, "Не совпали Rev");
        }

        public static void EqualDto(UserDto expected, UserDto actual)
        {
            if (NUllComparer(expected, actual)) return;
            Assert.AreEqual(expected.ID, actual.ID, "Не совпали id у объекта UserDto");
            Assert.AreEqual(expected.Login, actual.Login, "Не совпали Login у объекта UserDto");
            Assert.AreEqual(expected.Name, actual.Name, "Не совпали Name у объекта UserDto");
            EqualDto(expected.Role, actual.Role);
        }

        public static void EqualDto(RoleDto expected, RoleDto actual)
        {
            if (NUllComparer(expected, actual)) return;
            Assert.AreEqual(expected.Name, actual.Name, "Не совпали Name у объекта RoleDto");
        }

        private static bool NUllComparer(object expected, object actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
                return true;
            }

            Assert.IsNotNull(actual, $"Переданный объект null! type = {expected.GetType().Name}");
            return false;
        }

        public static void EqualDto(ProjectDto expected, ProjectDto actual)
        {
            if (NUllComparer(expected, actual)) return;
            Assert.AreEqual(expected.ID, actual.ID, "Не совпали id у объекта ProjectDto");
            Assert.AreEqual(expected.Title, actual.Title, "Не совпали Title у объекта ProjectDto");
            EqualEnumerable(expected.Items, actual.Items, EqualDto);
        }

        private static void EqualDto(ItemDto expected, ItemDto actual)
        {
            if (NUllComparer(expected, actual)) return;
            Assert.AreEqual(expected.ID, actual.ID, "Не совпали id у объекта ItemDto");
            Assert.AreEqual(expected.ItemType, actual.ItemType, "Не совпали ItemType у объекта ItemDto");
            Assert.AreEqual(expected.ExternalItemId, actual.ExternalItemId, "Не совпали ExternalItemId у объекта ItemDto");
            Assert.AreEqual(expected.Name, actual.Name, "Не совпали Name у объекта ItemDto");
        }

        internal static void EqualISynchro(ISynchroTable expected, ISynchroTable actual)
        {
            if (expected is ItemSynchro)
            {
                if (!(actual is ItemSynchro))
                {
                    Assert.Fail("Типы полученых синхронизаторов не совподают!");
                }                
            }
            if (expected is UserSynchro)
            {
                if (!(actual is UserSynchro))
                {
                    Assert.Fail("Типы полученых синхронизаторов не совподают!");
                }
            }

            if (expected is ProjectSynchro)
            {
                if (!(actual is ProjectSynchro))
                {
                    Assert.Fail("Типы полученых синхронизаторов не совподают!");
                }
            }
        }

        

        public static void EqualEnumerable<T>(IEnumerable<T> expected, IEnumerable<T> actual, Action<T, T> equal)
        {
            if (NUllComparer(expected, actual)) return;
            EqualList(expected.ToList(), actual.ToList(), equal);
        }
    }
}
