using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Utility.Mapping;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        private static SharedDatabaseFixture Fixture { get; set; }

        private static UserService service;
        private static IMapper mapper;

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            mapper = mapperConfig.CreateMapper();
        }

        [TestInitialize]
        public void Setup()
        {
            Fixture = new SharedDatabaseFixture(context =>
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Users.AddRange(MockData.DEFAULT_USERS);
                context.SaveChanges();
            });

            service = new UserService(
                Fixture.Context,
                mapper,
                new CryptographyHelper(),
                Mock.Of<ILogger<UserService>>());
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public async Task ExistsById_ExistingUser_ReturnsTrue()
        {
            var existingUser = Fixture.Context.Users.FirstOrDefault();
            var existingUserId = new ID<UserDto>(existingUser.ID);

            var result = await service.Exists(existingUserId);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ExistsById_NotExistingUser_ReturnsFalse()
        {
            var notExistingUserId = ID<UserDto>.InvalidID;

            var result = await service.Exists(notExistingUserId);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ExistsByLogin_ExistingUser_ReturnsTrue()
        {
            var existingUser = Fixture.Context.Users.FirstOrDefault();
            var existingUserLogin = existingUser.Login;

            var result = await service.Exists(existingUserLogin);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ExistsByLogin_NotExistingUser_ReturnsFalse()
        {
            var notExistingUserLogin = "dsgfsdgsgfsgreg4334g";

            var result = await service.Exists(notExistingUserLogin);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Add_NewUser_ReturnsTheUser()
        {
            var userLogin = "login_Add_ExistingUser_ReturnsTheUser";
            var userPassword = "password_Add_ExistingUser_ReturnsTheUser";
            var userName = "name_Add_ExistingUser_ReturnsTheUser";
            var userToCreate = new UserToCreateDto(userLogin, userPassword, userName);

            var result = await service.Add(userToCreate);

            var resultId = (int)result;
            var addedUser = await Fixture.Context.Users.SingleAsync(u => u.Login == userLogin && u.Name == userName);
            var addedUserId = addedUser.ID;
            Assert.AreEqual(addedUserId, resultId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task Add_UserWithExistingLogin_RaisesInvalidDataException()
        {
            var firstLogin = "login_Add_UserWithExistingLogin_ReturnsTheUser";
            var firstPassword = "password1_Add_UserWithExistingLogin_ReturnsTheUser";
            var firstName = "name1_Add_UserWithExistingLogin_ReturnsTheUser";
            var secondPassword = "password222_Add_UserWithExistingLogin_ReturnsTheUser";
            var secondName = "name222_Add_UserWithExistingLogin_ReturnsTheUser";
            var firstUser = new UserToCreateDto(firstLogin, firstPassword, firstName);
            var secondUser = new UserToCreateDto(firstLogin, secondPassword, secondName);

            //try
            //{
                await service.Add(firstUser);
                await service.Add(secondUser);
            //}
            //finally
            //{
            //    Fixture.Context.Users.Remove(Fixture.Context.Users.First(u => u.Login == firstLogin));
            //}

            Assert.Fail();
        }

        [TestMethod]
        public async Task Delete_ExistingUser_ReturnsTrue()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync();
            var userLogin = existingUser.Login;

            var result = await service.Delete(new ID<UserDto>(existingUser.ID));

            Assert.IsTrue(result);
            //Fixture.Context.Users.Add(existingUser);
        }

        [TestMethod]
        public async Task Delete_NotExistingUser_ReturnsFalse()
        {
            var result = await service.Delete(ID<UserDto>.InvalidID);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task FindById_ExistingUser_ReturnsUser()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync();

            var result = await service.Find(new ID<UserDto>(existingUser.ID));

            Assert.IsNotNull(result);
            Assert.AreEqual(existingUser.ID, (int)result.ID);
        }

        [TestMethod]
        public async Task FindById_NotExistingUserId_ReturnsNull()
        {
            var result = await service.Find(ID<UserDto>.InvalidID);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FindByLogin_ExistingUser_ReturnsUser()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync();

            var result = await service.Find(existingUser.Login);

            Assert.IsNotNull(result);
            Assert.AreEqual(existingUser.Login, result.Login);
        }

        [TestMethod]
        public async Task FindByLogin_NotExistingUserLogin_ReturnsNull()
        {
            var notExistingLogin = $"notExistingLogin{Guid.NewGuid()}";

            var result = await service.Find(notExistingLogin);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Update_ExistingUserAndCorrectNewInfo_ReturnsTrue()
        {
            var newLogin = $"newValidLogin{Guid.NewGuid()}";
            var newName = "newName";
            var existingUser = await Fixture.Context.Users.FirstAsync();
            var updatingUser = new UserDto(new ID<UserDto>(existingUser.ID), newLogin, newName);

            var result = await service.Update(updatingUser);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Update_ExistingUserAndIncorrectNewLogin_ReturnsFalse()
        {
            var users = Fixture.Context.Users.ToList();
            if (users.Count < 2)
                Assert.Fail("Incorrect number of initiated users");
            var userOne = users.First();
            var userTwo = users.First(u => u != userOne);
            var updatingUser = new UserDto(new ID<UserDto>(userOne.ID), userTwo.Login, "");

            var result = await service.Update(updatingUser);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Update_NotExistingUser_ReturnsFalse()
        {
            var updatingUser = new UserDto(ID<UserDto>.InvalidID, "", "");

            var result = await service.Update(updatingUser);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UpdatePassword_ExistingUser_ReturnsTrueAndPasswordUpdated()
        {
            var newPass = "newPass";
            var passHash = Guid.NewGuid().ToByteArray();
            var passSalt = Guid.NewGuid().ToByteArray();
            var mockedCryptographyHelper = new Mock<CryptographyHelper>();
            mockedCryptographyHelper.Setup(m => m.CreatePasswordHash(newPass, out passHash, out passSalt));
            var m_service = new UserService(
                Fixture.Context,
                mapper,
                mockedCryptographyHelper.Object,
                Mock.Of<ILogger<UserService>>());
            var existingUser = await Fixture.Context.Users.FirstAsync();
            var existingUserId = existingUser.ID;

            var result = await m_service.UpdatePassword(new ID<UserDto>(existingUserId), newPass);

            var updatedUser = await Fixture.Context.Users.FirstAsync(u => u.ID == existingUserId);
            var updatedHash = updatedUser.PasswordHash;
            var updatedSalt = updatedUser.PasswordSalt;
            Assert.IsTrue(result);
            Assert.AreEqual(passHash, updatedHash);
            Assert.AreEqual(passSalt, updatedSalt);
        }

        [TestMethod]
        public async Task UpdatePassword_NotExistingUser_ReturnsFalse()
        {
            var newPass = "newPass";
            var passHash = new byte[8];
            var passSalt = new byte[10];
            var mockedCryptographyHelper = new Mock<CryptographyHelper>();
            mockedCryptographyHelper.Setup(m => m.CreatePasswordHash(newPass, out passHash, out passSalt));
            var m_service = new UserService(
                Fixture.Context,
                mapper,
                mockedCryptographyHelper.Object,
                Mock.Of<ILogger<UserService>>());

            var result = await m_service.UpdatePassword(ID<UserDto>.InvalidID, newPass);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task VerifyPassword_ExistingUserAndCorrectPass_ReturnsTrue()
        {
            var pass = "pass";
            var existingUser = await Fixture.Context.Users.FirstAsync();
            var mockedCryptographyHelper = new Mock<CryptographyHelper>();
            mockedCryptographyHelper.Setup(m => m.VerifyPasswordHash(pass, existingUser.PasswordHash, existingUser.PasswordSalt))
                                    .Returns(true);
            var m_service = new UserService(
                Fixture.Context,
                mapper,
                mockedCryptographyHelper.Object,
                Mock.Of<ILogger<UserService>>());

            var result = await m_service.VerifyPassword(new ID<UserDto>(existingUser.ID), pass);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task VerifyPassword_ExistingUserAndIncorrectPass_ReturnsFalse()
        {
            var pass = "pass";
            var passHash = new byte[8];
            var passSalt = new byte[10];
            var mockedCryptographyHelper = new Mock<CryptographyHelper>();
            mockedCryptographyHelper.Setup(m => m.VerifyPasswordHash(pass, passHash, passSalt)).Returns(false);
            var m_service = new UserService(
                Fixture.Context,
                mapper,
                mockedCryptographyHelper.Object,
                Mock.Of<ILogger<UserService>>());
            var existingUser = await Fixture.Context.Users.FirstAsync();
            var existingUserId = existingUser.ID;

            var result = await m_service.VerifyPassword(new ID<UserDto>(existingUserId), pass);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task VerifyPassword_NotExistingUser_ReturnsFalse()
        {
            var pass = "pass";

            var result = await service.VerifyPassword(ID<UserDto>.InvalidID, pass);

            Assert.IsFalse(result);
        }
    }
}
