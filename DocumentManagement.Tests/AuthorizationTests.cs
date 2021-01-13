﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class AuthorizationTests
    {
        private static SharedDatabaseFixture Fixture { get; set; }

        private static AuthorizationService service;
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

                var users = MockData.DEFAULT_USERS;
                var roles = MockData.DEFAULT_ROLES;
                context.Users.AddRange(users);
                context.Roles.AddRange(roles);
                context.SaveChanges();

                if (users.Count >= 3 && roles.Count >= 2)
                {
                    var userRoles = new List<UserRole>
                    {
                        new UserRole { UserID = users[0].ID, RoleID = roles[0].ID },
                        new UserRole { UserID = users[1].ID, RoleID = roles[0].ID },
                        new UserRole { UserID = users[2].ID, RoleID = roles[1].ID }
                    };
                    context.UserRoles.AddRange(userRoles);
                    context.SaveChanges();
                }
            });

            service = new AuthorizationService(Fixture.Context, mapper, new CryptographyHelper());
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public async Task AddRole_ExistingUserAndExistingRole_ReturnsTrueWithoutAddingRole()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync(u => u.Roles.Count == 1);
            var userId = existingUser.ID;
            var currentRole = existingUser.Roles.First().Role;
            var roleToAdd = Fixture.Context.Roles.First(r => r != currentRole);
            var rolesCount = Fixture.Context.Roles.Count();

            var result = await service.AddRole(new ID<UserDto>(userId), roleToAdd.Name);

            Assert.IsTrue(result);
            Assert.IsTrue(existingUser.Roles.Any(r => r.Role == roleToAdd));
            Assert.AreEqual(rolesCount, Fixture.Context.Roles.Count());
        }

        [TestMethod]
        public async Task AddRole_ExistingUserAndNotExistingRole_ReturnsTrueAndAddsRole()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync(u => u.Roles.Count == 1);
            var userId = existingUser.ID;
            var currentRole = existingUser.Roles.First().Role;
            var roleToAdd = $"newRole{Guid.NewGuid()}";
            var rolesCount = Fixture.Context.Roles.Count();

            var result = await service.AddRole(new ID<UserDto>(userId), roleToAdd);

            Assert.IsTrue(result);
            Assert.IsTrue(existingUser.Roles.Any(r => r.Role.Name == roleToAdd));
            Assert.AreEqual(rolesCount + 1, Fixture.Context.Roles.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddRole_NotExistingUser_RaisesArgumentException()
        {
            var userId = ID<UserDto>.InvalidID;
            var role = "admin";

            await service.AddRole(userId, role);

            Assert.Fail();
        }

        [TestMethod]
        public async Task AddRole_UserAlreadyInRole_ReturnsFalse()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync(u => u.Roles.Count > 0);
            var usersId = new ID<UserDto>(existingUser.ID);
            var usersRole = existingUser.Roles.First().Role.Name;

            var result = await service.AddRole(usersId, usersRole);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetAllRoles_NormalWay_ReturnsAllRolesNames()
        {
            var contextRoles = Fixture.Context.Roles.Select(r => r.Name).ToList();

            var result = await service.GetAllRoles();

            contextRoles.ForEach(r =>
            {
                Assert.IsTrue(result.Any(resRole => resRole.Equals(r)));
            });
        }

        [TestMethod]
        public async Task GetUserRoles_ExistingUser_ReturnsAllUsersRolesNames()
        {
            var existingUser = Fixture.Context.Users.First(u => u.Roles.Count > 0);
            var existingUserRolesNames = existingUser.Roles.Select(r => r.Role.Name).ToList();

            var result = await service.GetUserRoles(new ID<UserDto>(existingUser.ID));

            existingUserRolesNames.ForEach(r =>
            {
                Assert.IsTrue(result.Any(resRole => resRole.Equals(r)));
            });
        }

        [TestMethod]
        public async Task GetUserRoles_NotExistingUser_ReturnsEmptyEnumerable()
        {
            var result = await service.GetUserRoles(ID<UserDto>.InvalidID);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public async Task IsInRole_ExistingUserInTheRole_ReturnsTrue()
        {
            var existingUser = Fixture.Context.Users.First(u => u.Roles.Count > 0);
            var userRole = existingUser.Roles.First().Role.Name;

            var result = await service.IsInRole(new ID<UserDto>(existingUser.ID), userRole);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsInRole_ExistingUserNotInTheRole_ReturnsFalse()
        {
            var existingUser = Fixture.Context.Users.First(u => u.Roles.Count == 1);
            var userRole = existingUser.Roles.First().Role;
            var notUserRole = Fixture.Context.Roles.First(r => r != userRole);

            var result = await service.IsInRole(new ID<UserDto>(existingUser.ID), notUserRole.Name);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsInRole_NotExistingUser_ReturnsFalse()
        {
            var role = Fixture.Context.Roles.First();

            var result = await service.IsInRole(ID<UserDto>.InvalidID, role.Name);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task RemoveRole_RoleWithExistingSingleUser_ReturnsTrueAndRemovesRoleWithEmptyUsersList()
        {
            var singleUserRole = Fixture.Context.UserRoles.First(r => r.Role.Users.Count == 1).Role;
            var user = singleUserRole.Users.First().User;

            var result = await service.RemoveRole(new ID<UserDto>(user.ID), singleUserRole.Name);

            Assert.IsTrue(result);
            Assert.IsFalse(user.Roles.Any(r => r.Role == singleUserRole));
            Assert.IsFalse(Fixture.Context.Roles.Any(r => r == singleUserRole));
            Assert.IsFalse(Fixture.Context.UserRoles.Any(ur => ur.Role == singleUserRole));
        }

        [TestMethod]
        public async Task RemoveRole_RoleWithExistingMultipleUser_ReturnsTrueAndDoenstRemoveRole()
        {
            var multipleUsersRole = Fixture.Context.UserRoles.First(r => r.Role.Users.Count > 1).Role;
            var user = multipleUsersRole.Users.First().User;

            var result = await service.RemoveRole(new ID<UserDto>(user.ID), multipleUsersRole.Name);

            Assert.IsTrue(result);
            Assert.IsFalse(user.Roles.Any(r => r.Role == multipleUsersRole));
            Assert.IsTrue(Fixture.Context.Roles.Contains(multipleUsersRole));
            Assert.IsTrue(Fixture.Context.UserRoles.Any(ur => ur.Role == multipleUsersRole));
        }

        [TestMethod]
        public async Task RemoveRole_NotExistingUser_ReturnsFalseAndDoesntRemoveRole()
        {
            var role = Fixture.Context.Roles.First();

            var result = await service.RemoveRole(ID<UserDto>.InvalidID, role.Name);

            Assert.IsFalse(result);
            Assert.IsTrue(Fixture.Context.Roles.Contains(role));
            Assert.IsTrue(Fixture.Context.UserRoles.Any(ur => ur.Role == role));
        }

        [TestMethod]
        public async Task RemoveRole_ExistingUserWithoutRole_ReturnsFalseAndDoesntRemoveRole()
        {
            var existingUser = Fixture.Context.Users.First(u => u.Roles.Count == 1);
            var userRole = existingUser.Roles.First().Role;
            var notUserRole = Fixture.Context.Roles.First(r => r != userRole);

            var result = await service.RemoveRole(new ID<UserDto>(existingUser.ID), notUserRole.Name);

            Assert.IsFalse(result);
            Assert.IsTrue(Fixture.Context.Roles.Contains(notUserRole));
            Assert.IsTrue(Fixture.Context.UserRoles.Any(ur => ur.Role == notUserRole));
        }

        [TestMethod]
        public async Task Login_ExistingUserWithCorrectPasswordAndWithRole_ReturnsValidatedUserWithRoles()
        {
            var user = Fixture.Context.Users.First(u => u.Roles.Any());
            var username = user.Login;
            var password = "pass";
            var userPassHash = user.PasswordHash;
            var userPassSalt = user.PasswordSalt;
            var mockedCryptographyHelper = new Mock<CryptographyHelper>();
            mockedCryptographyHelper.Setup(m => m.VerifyPasswordHash(password, userPassHash, userPassSalt)).Returns(true);
            var mockedService = new AuthorizationService(Fixture.Context, mapper, mockedCryptographyHelper.Object);

            var result = await mockedService.Login(username, password);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValidationSuccessful);
            Assert.IsTrue(result.User.Login == username);
            Assert.AreEqual(user.Roles.First().Role.Name, result.User.Role.Name);
        }

        [TestMethod]
        public async Task Login_NotExistingUser_ReturnsNull()
        {
            var username = $"notExistingLogin{Guid.NewGuid()}";
            var password = "pass";

            var result = await service.Login(username, password);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Login_ExistingUserWithInvalidPassword_ReturnsNull()
        {
            var user = Fixture.Context.Users.First(u => u.Roles.Any());
            var username = user.Login;
            var password = "pass";
            var userPassHash = user.PasswordHash;
            var userPassSalt = user.PasswordSalt;
            var mockedCryptographyHelper = new Mock<CryptographyHelper>();
            mockedCryptographyHelper.Setup(m => m.VerifyPasswordHash(password, userPassHash, userPassSalt)).Returns(false);
            var mockedService = new AuthorizationService(Fixture.Context, mapper, mockedCryptographyHelper.Object);

            var result = await mockedService.Login(username, password);

            Assert.IsNull(result);
        }
    }
}
