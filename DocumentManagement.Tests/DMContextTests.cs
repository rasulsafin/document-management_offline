using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using DocumentManagement.Tests.Utility;

namespace DocumentManagement.Tests
{
    [TestClass]
    public class DMContextTests
    {
        public static SharedDatabaseFixture Fixture { get; private set; }

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            Fixture = new SharedDatabaseFixture();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Fixture.Dispose();
        }

        [TestMethod]
        public void Can_add_user()
        {
            // Disposing a transaction forces it to be rolled back at the end of test
            // thus maintaining database state intact
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                var adminUser = MockData.AdminUser;
                int addedUserID = -1;

                using (var context = Fixture.CreateContext(transaction))
                {
                    context.Users.Add(adminUser);
                    var entryCount = context.SaveChanges();

                    Assert.AreEqual(1, entryCount);
                    addedUserID = adminUser.ID;
                }

                Assert.IsTrue(addedUserID > 0);

                using (var context = Fixture.CreateContext(transaction))
                {
                    var retrieved = context.Users.Find(addedUserID);
                    Assert.IsNotNull(retrieved);
                    
                    Assert.AreEqual(adminUser.Login, retrieved.Login);
                    CollectionAssert.AreEqual(adminUser.PasswordHash, retrieved.PasswordHash);
                    CollectionAssert.AreEqual(adminUser.PasswordSalt, retrieved.PasswordSalt);
                    Assert.AreEqual(adminUser.Name, retrieved.Name);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DbUpdateException), "Error: can add User with duplicate login")]
        public void Can_not_add_user_with_same_login()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    var user = MockData.AdminUser;
                    context.Users.Add(user);
                    context.SaveChanges();

                    var secondUser = MockData.OperatorUser;
                    secondUser.Login = user.Login;
                    context.Users.Add(secondUser);
                    context.SaveChanges();  //expect exception
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DbUpdateException), "Error: can assign duplicated login to User")]
        public void Can_not_modify_user_login_to_be_duplicated()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    var user = MockData.AdminUser;
                    context.Users.Add(user);

                    var secondUser = MockData.OperatorUser;
                    context.Users.Add(secondUser);
                    context.SaveChanges();

                    secondUser.Login = user.Login;
                    context.SaveChanges();  //expect exception
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void Can_assign_ConnectionInfo_to_user()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    var user = MockData.AdminUser;
                    context.Users.Add(user);

                    var connInfo = MockData.TDMSConnectionInfo;
                    context.ConnectionInfos.Add(connInfo);
                    context.SaveChanges();

                    Assert.IsTrue(user.ID > 0);
                    Assert.IsTrue(connInfo.ID > 0);

                    user.ConnectionInfoID = connInfo.ID;
                    context.SaveChanges();
                }

                using (var context = Fixture.CreateContext(transaction))
                {
                    var user = context.Users.Include(x => x.ConnectionInfo)
                        .FirstOrDefault(x => x.Login == MockData.AdminUser.Login);
                    Assert.IsTrue(user.ConnectionInfoID > 0);
                    Assert.IsNotNull(user.ConnectionInfo);
                    Assert.AreEqual(MockData.TDMSConnectionInfo.AuthFieldNames, user.ConnectionInfo.AuthFieldNames);
                    Assert.AreEqual(MockData.TDMSConnectionInfo.Name, user.ConnectionInfo.Name);
                }
            }
        }

        [TestMethod]
        public void Can_remove_ConnectionInfo_and_update_linked_users()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    var connInfo = MockData.TDMSConnectionInfo;
                    context.ConnectionInfos.Add(connInfo);
                    context.SaveChanges();

                    var user1 = MockData.AdminUser;
                    user1.ConnectionInfoID = connInfo.ID;
                    context.Users.Add(user1);

                    var user2 = MockData.OperatorUser;
                    user1.ConnectionInfoID = connInfo.ID;
                    context.Users.Add(user2);

                    context.SaveChanges();

                    context.ConnectionInfos.Remove(connInfo);
                    context.SaveChanges();

                    Assert.IsNull(user1.ConnectionInfoID);
                    Assert.IsNull(user2.ConnectionInfoID);
                }
            }
        }

        [TestMethod]
        public void Are_EnumDms_and_EnumDmValues_removed_with_ConnectionInfo()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    var connInfo = MockData.TDMSConnectionInfo;
                    context.ConnectionInfos.Add(connInfo);
                    context.SaveChanges();

                    Assert.IsTrue(connInfo.ID > 0);

                    var enumDms = MockData.CreateEnumDms("TDMS", connInfo.ID).ToList();
                    context.EnumDms.AddRange(enumDms);
                    context.SaveChanges();

                    foreach (var item in enumDms)
                    {
                        Assert.IsTrue(item.ID > 0);
                        var values = MockData.CreateEnumDmValues(item.ID, "TDMS").ToList();
                        context.EnumDmValues.AddRange(values);
                    }
                    context.SaveChanges();

                    Assert.AreEqual(3, context.EnumDms.Count());
                    Assert.AreEqual(9, context.EnumDmValues.Count());

                    context.ConnectionInfos.Remove(connInfo);
                    context.SaveChanges();

                    Assert.AreEqual(0, context.EnumDms.Count());
                    Assert.AreEqual(0, context.EnumDmValues.Count());
                }
            }
        }
    }
}
