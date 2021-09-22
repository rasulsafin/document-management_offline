using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database.Models;
using Brio.Docs.Interface.Dtos;
using Brio.Docs.Services;
using Brio.Docs.Tests.Utility;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Brio.Docs.Exceptions;

namespace Brio.Docs.Tests
{
    [TestClass]
    public class ProjectServiceTests : IDisposable
    {
        private static IMapper mapper;
        private ProjectService service;
        private SharedDatabaseFixture fixture;

        [ClassInitialize]
        public static void ClassSetup(TestContext unused)
        {
            var mapperConfig = new MapperConfiguration(
                mc =>
                {
                    mc.AddProfile(new MappingProfile());
                });
            mapper = mapperConfig.CreateMapper();
        }

        [TestInitialize]
        public void Setup()
        {
            fixture = new SharedDatabaseFixture(
                context =>
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.Projects.AddRange(MockData.DEFAULT_PROJECTS);
                    context.Users.AddRange(MockData.DEFAULT_USERS);
                    context.SaveChanges();
                });
            service = new ProjectService(
                fixture.Context,
                mapper,
                new ItemHelper(Mock.Of<ILogger<ItemHelper>>()),
                Mock.Of<ILogger<ProjectService>>());
        }

        [TestCleanup]
        public void Cleanup()
            => Dispose();

        [TestMethod]
        public async Task Add_CorrectInfo_AddsProject()
        {
            var project = new ProjectToCreateDto
            {
                Title = "project",
                Items = new List<ItemDto>(),
            };

            var added = await service.Add(project);

            Assert.IsNotNull(added);
            Assert.IsNotNull(added.ID);
        }

        [TestMethod]
        public async Task Add_CorrectInfoWithOwner_AddsProjectToOwner()
        {
            var owner = fixture.Context.Users.Include(x => x.Projects).First();
            var project = new ProjectToCreateDto
            {
                Title = "project",
                AuthorID = new ID<UserDto>(owner.ID),
                Items = new List<ItemDto>(),
            };

            var added = await service.Add(project);

            Assert.IsNotNull(added);
            Assert.IsNotNull(added.ID);
            Assert.AreEqual(owner.Projects.Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Add_InvalidTitle_RaisesArgumentException()
        {
            var project = new ProjectToCreateDto
            {
                Title = string.Empty,
                Items = new List<ItemDto>(),
            };

            await service.Add(project);

            Assert.Fail();
        }

        [TestMethod]
        public async Task Find_ProjectExists_ReturnsProject()
        {
            var project = await fixture.Context.Projects.FirstAsync();
            var id = new ID<ProjectDto>(project.ID);

            var projectFound = await service.Find(id);

            Assert.IsNotNull(projectFound);
            Assert.AreEqual(project.ID, (int)projectFound.ID);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task Find_ProjectDoesntExist_RaisesNotFoundException()
        {
            var id = ID<ProjectDto>.InvalidID;

            await service.Find(id);

            Assert.Fail();
        }

        [TestMethod]
        public async Task GetAllProjects_ProjectsExist_ReturnsProjects()
        {
            var projectsFound = await service.GetAllProjects();

            Assert.IsNotNull(projectsFound);

            Assert.AreEqual(await fixture.Context.Projects.CountAsync(), projectsFound.Count());
        }

        [TestMethod]
        public async Task GetUserProjects_UserExists_ReturnsProjects()
        {
            var user = fixture.Context.Users.Include(x => x.Projects).First();
            var projects = await fixture.Context.Projects.ToListAsync();
            user.Projects = projects.Select(x => new UserProject { Project = x }).ToList();
            fixture.Context.Users.Update(user);
            await fixture.Context.SaveChangesAsync();
            var id = new ID<UserDto>(user.ID);

            var foundProjects = await service.GetUserProjects(id);

            Assert.IsNotNull(foundProjects);
            Assert.AreEqual(projects.Count, foundProjects.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task GetUserProjects_UserDoesntExist_RaisesNotFoundException()
        {
            var id = ID<UserDto>.InvalidID;

            await service.GetUserProjects(id);

            Assert.Fail();
        }

        [TestMethod]
        public async Task GetUsers_ProjectExists_ReturnsUsers()
        {
            var users = await fixture.Context.Users.ToListAsync();
            var project = await fixture.Context.Projects.FirstAsync();
            project.Users = users.Select(x => new UserProject { User = x }).ToList();
            fixture.Context.Projects.Update(project);
            await fixture.Context.SaveChangesAsync();
            var id = new ID<ProjectDto>(project.ID);

            var foundUsers = await service.GetUsers(id);

            Assert.IsNotNull(foundUsers);
            Assert.AreEqual(users.Count, foundUsers.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task GetUsers_ProjectDoesntExist_RaisesNotFoundException()
        {
            await service.GetUsers(ID<ProjectDto>.InvalidID);

            Assert.Fail();
        }

        [TestMethod]
        public async Task LinkToUsers_ProjectExistsUsersExist_LinksUsers()
        {
            var project = await fixture.Context.Projects.FirstAsync();
            var projectID = new ID<ProjectDto>(project.ID);
            var usersIDs = await fixture.Context.Users.Select(x => new ID<UserDto>(x.ID)).ToArrayAsync();

            var result = await service.LinkToUsers(projectID, usersIDs);

            Assert.IsTrue(result);
            Assert.AreEqual(await fixture.Context.Users.CountAsync(), project.Users.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task LinkToUsers_ProjectDoesntExists_RaisesNotFoundException()
        {
            var projectID = ID<ProjectDto>.InvalidID;
            var usersIDs = await fixture.Context.Users.Select(x => new ID<UserDto>(x.ID)).ToArrayAsync();

            await service.LinkToUsers(projectID, usersIDs);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task LinkToUsers_UserDoesntExists_RaisesNotFoundException()
        {
            var project = await fixture.Context.Projects.FirstAsync();
            var projectID = new ID<ProjectDto>(project.ID);
            var usersIDs = fixture.Context.Users.Select(x => new ID<UserDto>(x.ID))
               .AsEnumerable()
               .Append(ID<UserDto>.InvalidID);

            await service.LinkToUsers(projectID, usersIDs);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task LinkToUsers_UsersDoesntExists_RaisesNotFoundException()
        {
            var project = await fixture.Context.Projects.FirstAsync();
            var projectID = new ID<ProjectDto>(project.ID);
            var usersIDs = new[] { ID<UserDto>.InvalidID, new ID<UserDto>(int.MaxValue) };

            await service.LinkToUsers(projectID, usersIDs);

            Assert.Fail();
        }

        [TestMethod]
        public async Task Remove_ProjectExists_RemovesProject()
        {
            var project = await fixture.Context.Projects.FirstAsync();
            var projectsCount = await fixture.Context.Projects.CountAsync();
            var id = new ID<ProjectDto>(project.ID);

            var result = await service.Remove(id);

            Assert.IsTrue(result);
            Assert.AreEqual(projectsCount - 1, await fixture.Context.Projects.CountAsync());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task Remove_ProjectDoesntExist_RaisesNotFoundException()
        {
            var id = ID<ProjectDto>.InvalidID;

            await service.Remove(id);

            Assert.Fail();
        }

        [TestMethod]
        public async Task UnlinkFromUsers_ProjectExistsUsersExist_UnlinkLinksUsers()
        {
            var users = await fixture.Context.Users.ToListAsync();
            var project = await fixture.Context.Projects.FirstAsync();
            project.Users = users.Select(x => new UserProject { User = x }).ToList();
            fixture.Context.Projects.Update(project);
            await fixture.Context.SaveChangesAsync();

            var projectID = new ID<ProjectDto>(project.ID);
            var usersIDs = await fixture.Context.Users.Select(x => new ID<UserDto>(x.ID)).ToArrayAsync();

            var result = await service.UnlinkFromUsers(projectID, usersIDs);

            Assert.IsTrue(result);
            Assert.AreEqual(0, project.Users.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task UnlinkFromUsers_ProjectDoesntExists_RaisesNotFoundException()
        {
            var projectID = ID<ProjectDto>.InvalidID;
            var usersIDs = await fixture.Context.Users.Select(x => new ID<UserDto>(x.ID)).ToArrayAsync();

            await service.UnlinkFromUsers(projectID, usersIDs);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task UnlinkFromUsers_UserDoesntExists_RaisesNotFoundException()
        {
            var project = await fixture.Context.Projects.FirstAsync();
            var projectID = new ID<ProjectDto>(project.ID);
            var usersIDs = fixture.Context.Users.Select(x => new ID<UserDto>(x.ID))
               .AsEnumerable()
               .Append(ID<UserDto>.InvalidID);

            await service.UnlinkFromUsers(projectID, usersIDs);

            Assert.Fail();
        }

        [TestMethod]
        public async Task Update_ProjectExistsTitleIsValid_UpdatesProject()
        {
            var project = await fixture.Context.Projects.FirstAsync();
            var id = new ID<ProjectDto>(project.ID);
            var newTitle = "New title";
            var projectDto = new ProjectDto
            {
                ID = id,
                Items = new List<ItemDto>(),
                Title = newTitle,
            };

            var result = await service.Update(projectDto);

            Assert.IsTrue(result);
            Assert.AreEqual(newTitle, project.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Update_ProjectExistsTitleIsInvalid_RaisesArgumentException()
        {
            var project = await fixture.Context.Projects.FirstAsync();
            var id = new ID<ProjectDto>(project.ID);
            var projectDto = new ProjectDto
            {
                ID = id,
                Items = new List<ItemDto>(),
                Title = string.Empty,
            };

            await service.Update(projectDto);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task Update_ProjectDoesntExist_RaisesNotFoundException()
        {
            var id = ID<ProjectDto>.InvalidID;
            var project = new ProjectDto
            {
                ID = id,
                Items = new List<ItemDto>(),
                Title = "Title",
            };

            await service.Update(project);

            Assert.Fail();
        }

        public void Dispose()
        {
            fixture.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
