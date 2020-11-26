using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;
using DocumentManagement.Tests.Utility;

namespace DocumentManagement.Tests
{
    [TestClass]
    public class ProjectServiceTests
    {
        public static SharedDatabaseFixture Fixture { get; private set; }

        [ClassInitialize]
        public static void Setup(TestContext _)
        {
            Fixture = new SharedDatabaseFixture();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Fixture.Dispose();
        }

        [TestMethod]
        public async Task Can_add_user_specific_projects()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreate("vpupkin", "123", "Vasily Pupkin"));
                var currentUser = access.CurrentUser.ID;
                var user1 = await access.UserService.Add(new UserToCreate("itaranov", "123", "Ivan Taranov"));
                var user2 = await access.UserService.Add(new UserToCreate("ppshezdetsky", "123", "Pshek Pshezdetsky"));

                var projectService = access.ProjectService;

                var commonProject = await projectService.Add(currentUser, "Common project");
                var u1Project = await projectService.Add(user1, "Project for user 1");
                var u2Project = await projectService.Add(user2, "Project for user 2");
                var u12Project = await projectService.Add(user1, "Project for users 1 and 2");

                await projectService.AddUsers(commonProject, new[] { currentUser, user1, user2 });
                await projectService.AddUsers(u1Project, new[] { user1 });
                await projectService.AddUsers(u2Project, new[] { user2 });
                await projectService.AddUsers(u12Project, new[] { user2 });

                var currentUserProjects = await projectService.GetUserProjects(access.CurrentUser.ID);
                var user1Projects = await projectService.GetUserProjects(user1);
                var user2Projects = await projectService.GetUserProjects(user2);

                var expectedProjects = new Project[]
                {
                    new Project { ID = commonProject, Title = "Common project" }
                };
                CollectionAssert.That.AreEquivalent(expectedProjects, currentUserProjects, new ProjectComparer());

                expectedProjects = new Project[]
                {
                    new Project { ID = commonProject, Title = "Common project" },
                    new Project { ID = u1Project, Title = "Project for user 1" },
                    new Project { ID = u12Project, Title = "Project for users 1 and 2" }
                };
                CollectionAssert.That.AreEquivalent(expectedProjects, user1Projects, new ProjectComparer());

                expectedProjects = new Project[]
                {
                    new Project { ID = commonProject, Title = "Common project" },
                    new Project { ID = u2Project, Title = "Project for user 2" },
                    new Project { ID = u12Project, Title = "Project for users 1 and 2" }
                };
                CollectionAssert.That.AreEquivalent(expectedProjects, user2Projects, new ProjectComparer());
            }
        }

        [TestMethod]
        public async Task Can_query_project_users()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreate("vpupkin", "123", "Vasily Pupkin"));
                var currentUserID = access.CurrentUser.ID;
                var user1ID = await access.UserService.Add(new UserToCreate("itaranov", "123", "Ivan Taranov"));
                var user2ID = await access.UserService.Add(new UserToCreate("ppshezdetsky", "123", "Pshek Pshezdetsky"));
                var projectService = access.ProjectService;

                var commonProject = await projectService.Add(currentUserID, "Common project");
                var u1Project = await projectService.Add(user1ID, "Project for user 1");
                var u2Project = await projectService.Add(user2ID, "Project for user 2");
                var u12Project = await projectService.Add(user1ID, "Project for users 1 and 2");

                //do not add users to common project - project without owner should be visible to anyone
                await projectService.AddUsers(commonProject, new[] { user1ID, user2ID });
                await projectService.AddUsers(u12Project, new[] { user2ID });

                var currentUser = access.CurrentUser;
                var user1 = await access.UserService.Find(user1ID);
                var user2 = await access.UserService.Find(user2ID);

                var commonUsers = await projectService.GetUsers(commonProject);
                CollectionAssert.That.AreEquivalent(new User[] { currentUser, user1, user2 }, commonUsers, new UserComparer(ignoreIDs: false));

                var project1Users = await projectService.GetUsers(u1Project);
                CollectionAssert.That.AreEquivalent(new User[] { user1 }, project1Users, new UserComparer(ignoreIDs: false));

                var project2Users = await projectService.GetUsers(u2Project);
                CollectionAssert.That.AreEquivalent(new User[] { user2 }, project2Users, new UserComparer(ignoreIDs: false));

                var project12Users = await projectService.GetUsers(u12Project);
                CollectionAssert.That.AreEquivalent(new User[] { user1, user2 }, project12Users, new UserComparer(ignoreIDs: false));
            }
        }

        [TestMethod]
        public async Task Can_remove_project_users()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreate("vpupkin", "123", "Vasily Pupkin"));

                var currentUserID = access.CurrentUser.ID;
                var user1ID = await access.UserService.Add(new UserToCreate("itaranov", "123", "Ivan Taranov"));
                
                var projectService = access.ProjectService;
                
                var project1 = await projectService.Add(currentUserID, "Project 1");
                var project2 = await projectService.Add(user1ID, "Project 2");
                
                var project12 = await projectService.Add(currentUserID, "Project 1 and 2");
                await projectService.AddUsers(project12, new[] { user1ID });

                var currentUser = access.CurrentUser;
                var user1 = await access.UserService.Find(user1ID);

                await projectService.RemoveUsers(project1, new[] { currentUserID });
                await projectService.RemoveUsers(project12, new[] { user1ID });

                var project1Users = await projectService.GetUsers(project1);
                CollectionAssert.That.AreEquivalent(new User[] { }, project1Users, new UserComparer(ignoreIDs: false));

                var project2Users = await projectService.GetUsers(project2);
                CollectionAssert.That.AreEquivalent(new User[] { user1 }, project2Users, new UserComparer(ignoreIDs: false));

                var project12Users = await projectService.GetUsers(project12);
                CollectionAssert.That.AreEquivalent(new User[] { currentUser }, project12Users, new UserComparer(ignoreIDs: false));
            }
        }

        [TestMethod]
        public async Task Can_query_projects()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreate("vpupkin", "123", "Vasily Pupkin"));

                var currentUserID = access.CurrentUser.ID;
                var projectService = access.ProjectService;

                var project1 = await projectService.Add(currentUserID, "Project 1");
                var project2 = await projectService.Add(currentUserID, "Project 2");
                var project3 = await projectService.Add(currentUserID, "Project 3");

                var p1 = await projectService.Find(project1);
                Assert.IsTrue(new ProjectComparer().Equals(p1, new Project() { ID = project1, Title = "Project 1" }));

                await projectService.RemoveUsers(project3, new[] { currentUserID });

                var userProjects = await projectService.GetUserProjects(currentUserID);
                CollectionAssert.That.AreEquivalent(new Project[] { 
                    new Project(){ ID = project1, Title = "Project 1" },
                    new Project(){ ID = project2, Title = "Project 2" }
                }, userProjects, new ProjectComparer());

                var allProjects = await projectService.GetAllProjects();
                CollectionAssert.That.AreEquivalent(new Project[] {
                    new Project(){ ID = project1, Title = "Project 1" },
                    new Project(){ ID = project2, Title = "Project 2" },
                    new Project(){ ID = project3, Title = "Project 3" }
                }, allProjects, new ProjectComparer());
            }
        }

        [TestMethod]
        public async Task Can_update_projects()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreate("vpupkin", "123", "Vasily Pupkin"));

                var currentUserID = access.CurrentUser.ID;
                var projectService = access.ProjectService;

                var project1 = await projectService.Add(currentUserID, "Project 1");
                var project2 = await projectService.Add(currentUserID, "Project 2");

                var expectedProjects = new Project[] 
                {
                    new Project() { ID = project1, Title = "New project 1" },
                    new Project() { ID = project2, Title = "New project 2" }
                };
                await projectService.Update(expectedProjects[0]);
                await projectService.Update(expectedProjects[1]);

                var userProjects = await projectService.GetUserProjects(currentUserID);
                CollectionAssert.That.AreEquivalent(expectedProjects, userProjects, new ProjectComparer());
            }
        }

        [TestMethod]
        public async Task Can_remove_projects()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreate("vpupkin", "123", "Vasily Pupkin"));

                var currentUserID = access.CurrentUser.ID;
                var projectService = access.ProjectService;

                var project1 = await projectService.Add(currentUserID, "Project 1");
                var project2 = await projectService.Add(currentUserID, "Project 2");
                var project3 = await projectService.Add(currentUserID, "Project 3");

                var deleted = await projectService.Remove(project1);
                Assert.IsTrue(deleted);

                var userProjects = await projectService.GetUserProjects(currentUserID);
                CollectionAssert.That.AreEquivalent(new Project[] {
                    new Project(){ ID = project2, Title = "Project 2" },
                    new Project(){ ID = project3, Title = "Project 3" }
                }, userProjects, new ProjectComparer());

                // Try to delete already deleted project
                deleted = await projectService.Remove(project1);
                Assert.IsFalse(deleted);
            }
        }
    }
}
