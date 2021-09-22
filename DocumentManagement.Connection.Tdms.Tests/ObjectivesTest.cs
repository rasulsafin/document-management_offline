using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.Tdms.Tests
{
    [TestClass]
    public class ObjectivesTest
    {
        private static readonly string TEST_FILE_PATH = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "IntegrationTestFile.txt");

        private static TdmsObjectivesSynchronizer objectiveService;
        private static ObjectiveExternalDto objectiveDefect;
        private static BimElementExternalDto bimElement;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            // TODO: Find job and find issues and use their guids to test?
            objectiveService = InitClass.connectionContext.ObjectivesSynchronizer as TdmsObjectivesSynchronizer;

            bimElement = new BimElementExternalDto()
            {
                GlobalID = Guid.NewGuid().ToString(),
            };

            var objectiveTypeDefect = new ObjectiveTypeExternalDto()
            {
                Name = ObjectTypeID.DEFECT,
            };

            var objectiveTypeWork = new ObjectiveTypeExternalDto()
            {
                Name = ObjectTypeID.WORK,
            };

            var item = new ItemExternalDto()
            {
                FileName = System.IO.Path.GetFileName(TEST_FILE_PATH),
                FullPath = TEST_FILE_PATH,
                ItemType = ItemType.File,
            };

            var dynamicFieldOne = new DynamicFieldExternalDto()
            {
                ExternalID = AttributeID.BUILDER,
                Type = DynamicFieldType.ENUM,
                Value = "{FA51CC89-881B-4753-958B-71AF73F75D85}",
            };

            var dynamicFieldTwo = new DynamicFieldExternalDto()
            {
                ExternalID = AttributeID.COMMENT,
                Type = DynamicFieldType.STRING,
                Value = "Comment",
            };

            objectiveDefect = new ObjectiveExternalDto()
            {
                ProjectExternalID = "{7B99CE45-95C5-4F59-801B-A4E96B547E84}",
                ParentObjectiveExternalID = "{8CA2DD65-3D8B-43FC-A1DA-4356BE509461}",
                AuthorExternalID = "USER_D3C57839_CC10_4AC6_83FB_F239A0F2C16E",
                ObjectiveType = objectiveTypeDefect,
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now,
                Title = "Objective to Create",
                Description = "Test objective creation",
                Status = ObjectiveStatus.InProgress,
                Items = new List<ItemExternalDto>() { item },
                DynamicFields = new List<DynamicFieldExternalDto> { dynamicFieldOne, dynamicFieldTwo },
                BimElements = new List<BimElementExternalDto>() { bimElement },
            };
        }

        [TestMethod]
        [DataRow("{C98B456D-AB4F-4D78-9748-4129DB8294D7}", DisplayName = "Get Defect")]
        [DataRow("{8CA2DD65-3D8B-43FC-A1DA-4356BE509461}", DisplayName = "Get Job")]
        public void GetObjective_ExistingObjectiveID_ReturnsObjectiveDto(string value)
        {
            var res = objectiveService.GetById(value);
            Assert.IsNotNull(res);
            Assert.AreEqual(res.Items.Count, 1);
        }

        [TestMethod]
        public async Task AddObjective_NonExistingObjectiveDefectType_ReturnsObjectiveDto()
        {
            var objectiveDto = await objectiveService.Add(objectiveDefect);
            Assert.IsNotNull(objectiveDto);
            Assert.IsNotNull(objectiveDto.ExternalID);
            Assert.AreEqual(objectiveDto.Items.Count, 1);
            Assert.AreEqual(objectiveDto.BimElements.Count, 1);

            // Remove added issue
            await objectiveService.Remove(objectiveDto);
        }

        [TestMethod]
        public async Task UpdateObjective_ExistingObjectiveDefectType_ReturnsObjectiveDto()
        {
            var oldValues = objectiveService.GetById("{C98B456D-AB4F-4D78-9748-4129DB8294D7}");

            var objective = objectiveService.GetById("{C98B456D-AB4F-4D78-9748-4129DB8294D7}");
            objective.CreationDate = objective.CreationDate.AddDays(1);
            objective.DueDate = objective.DueDate.AddDays(1);
            objective.Description += " (Updated)";
            objective.Title += " (Updated)";
            objective.BimElements.Add(bimElement);

            var updatedObjective = await objectiveService.Update(objective);
            Assert.AreEqual(objective.Description, updatedObjective.Description);
            Assert.AreEqual(objective.Title, updatedObjective.Title);
            Assert.AreEqual(objective.DueDate, updatedObjective.DueDate);
            Assert.AreEqual(objective.CreationDate, updatedObjective.CreationDate);

            Assert.IsNotNull(updatedObjective.BimElements);
            Assert.AreEqual(objective.BimElements.Count, updatedObjective.BimElements.Count);

            // Return previous values
            await objectiveService.Update(oldValues);
        }

        [TestMethod]
        public async Task RemoveObjective_ExistingObjectiveDefectType_ReturnsTrue()
        {
            var objectiveDto = await objectiveService.Add(objectiveDefect);
            var res = await objectiveService.Remove(objectiveDto);
            Assert.IsNotNull(res);
        }

        [TestMethod]
        public async Task GetUpdatedIDs_UpdatedYesterday_ReturnsUpdatedListWithMoreThanOneValue()
        {
            var objectiveDto = await objectiveService.Add(objectiveDefect);

            var res = await objectiveService.GetUpdatedIDs(DateTime.Now.AddDays(-1));
            Assert.IsTrue(res.Count > 0);

            // Remove added issue
            await objectiveService.Remove(objectiveDto);
        }

        [TestMethod]
        public async Task GetUpdatedIDs_UpdatedTomorrow_ReturnsEmptyList()
        {
            var res = await objectiveService.GetUpdatedIDs(DateTime.Now.AddDays(1));
            Assert.AreEqual(res.Count, 0);
        }

        [TestMethod]
        public async Task GetListOfObjectives_ReturnsListOfObjectiveDto()
        {
            var ids = new List<string>() { "{C98B456D-AB4F-4D78-9748-4129DB8294D7}", "{8CA2DD65-3D8B-43FC-A1DA-4356BE509461}" };
            var list = await objectiveService.Get(ids);
            Assert.IsNotNull(list);
            Assert.AreEqual(list.Count, 2);
        }
    }
}
