using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class ObjectiveTypeServiceTests
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
        public async Task Can_add_new_objective_type()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

                var taskTypeID = await access.ObjectiveTypeService.Add("Задание");
                var errorTypeID = await access.ObjectiveTypeService.Add("Нарушение");

                Assert.IsTrue(taskTypeID.IsValid);
                Assert.IsTrue(errorTypeID.IsValid);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task Can_not_add_duplicate_objective_type()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

                var id1 = await access.ObjectiveTypeService.Add("Задание");
                var id2 = await access.ObjectiveTypeService.Add("Задание");

                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task Can_query_objective_types()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

                var taskTypeID = await access.ObjectiveTypeService.Add("Задание");
                var errorTypeID = await access.ObjectiveTypeService.Add("Нарушение");

                var types = await access.ObjectiveTypeService.GetAllObjectiveTypes();
                var expected = new ObjectiveTypeDto[] 
                {
                    new ObjectiveTypeDto(){ ID = taskTypeID, Name = "Задание" },
                    new ObjectiveTypeDto(){ ID = errorTypeID, Name = "Нарушение" }
                };
                var comparer = new DelegateComparer<ObjectiveTypeDto>((x, y) => x.ID == y.ID && x.Name == y.Name);
                CollectionAssert.That.AreEquivalent(expected, types, comparer);

                var nonexsitent = await access.ObjectiveTypeService.Find(ID<ObjectiveTypeDto>.InvalidID);
                Assert.IsNull(nonexsitent);

                var item1 = await access.ObjectiveTypeService.Find(taskTypeID);
                Assert.IsNotNull(item1);
                Assert.IsTrue(comparer.Equals(expected[0], item1));

                var item2 = await access.ObjectiveTypeService.Find("Нарушение");
                Assert.IsNotNull(item2);
                Assert.IsTrue(comparer.Equals(expected[1], item2));
            }
        }

        [TestMethod]
        public async Task Can_remove_objective_types()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

                var taskTypeID = await access.ObjectiveTypeService.Add("Задание");
                var errorTypeID = await access.ObjectiveTypeService.Add("Нарушение");

                var isRemoved = await access.ObjectiveTypeService.Remove(taskTypeID);
                Assert.IsTrue(isRemoved);

                isRemoved = await access.ObjectiveTypeService.Remove(taskTypeID);
                Assert.IsFalse(isRemoved);

                var types = await access.ObjectiveTypeService.GetAllObjectiveTypes();
                var expected = new ObjectiveTypeDto[]
                {
                    new ObjectiveTypeDto(){ ID = errorTypeID, Name = "Нарушение" }
                };
                var comparer = new DelegateComparer<ObjectiveTypeDto>((x, y) => x.ID == y.ID && x.Name == y.Name);
                CollectionAssert.That.AreEquivalent(expected, types, comparer);
            }
        }
    }
}
