using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;

namespace DocumentManagement.Connection.Tests
{
    public class AssertHelper
    {
        public static void EqualSyncAction(SyncAction expected, SyncAction actual)
        {
            Assert.AreEqual(expected.ID, actual.ID, "Не совпали id");
            Assert.AreEqual(expected.Synchronizer, actual.Synchronizer, "Не совпали Synchronizer");
            Assert.AreEqual(expected.TypeAction, actual.TypeAction, "Не совпали TypeAction");
            Assert.AreEqual(expected.SpecialSynchronization, actual.SpecialSynchronization, "Не совпали SubSynchronization");
        }

        public static void EqualList<T>(List<T> expected, List<T> actual, Action<T, T> equalAction)
        {
            NUllComparer(expected, actual);            
            Assert.AreEqual(expected.Count, actual.Count, "Не совподает число элементов");
            for (int i = 0; i < actual.Count; i++)
            {
                equalAction(expected[i], actual[i]);
            }
        }


        public static void EqualRevisionCollection(RevisionCollection expected, RevisionCollection actual)
        {
            NUllComparer(expected, actual);
            EqualList(expected.Users, actual.Users, EqualRevision);
            EqualList(expected.Objectives, actual.Objectives, EqualRevisionChildsItem);
            EqualList(expected.Projects, actual.Projects, EqualRevisionChildsItem);
        }

        public static void EqualRevisionChildsItem(RevisionChildsItem expected, RevisionChildsItem actual)
        {
            NUllComparer(expected, actual);
            EqualRevision(expected, actual);
            EqualList(expected.Items, actual.Items, EqualRevision);
        }

        public static void EqualRevision(Revision expected, Revision actual)
        {
            NUllComparer(expected, actual);
            Assert.AreEqual(expected.ID, actual.ID, "Не совпали id");
            Assert.AreEqual(expected.Rev, actual.Rev, "Не совпали Rev");
        }

        public static void EqualDto(UserDto expected, UserDto actual)
        {
            NUllComparer(expected, actual);
            Assert.AreEqual(expected.ID, actual.ID, "Не совпали id у объекта UserDto");
            Assert.AreEqual(expected.Login, actual.Login, "Не совпали Login у объекта UserDto");
            Assert.AreEqual(expected.Name, actual.Name, "Не совпали Name у объекта UserDto");
            EqualDto(expected.Role, actual.Role);
        }

        private static void EqualDto(RoleDto expected, RoleDto actual)
        {
            NUllComparer(expected, actual);
            Assert.AreEqual(expected.Name, actual.Name, "Не совпали Name у объекта RoleDto");
        }

        private static void NUllComparer(object expected, object actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
                return;
            }

            Assert.IsNotNull(actual, "Переданная объект null");
        }

    }
}
