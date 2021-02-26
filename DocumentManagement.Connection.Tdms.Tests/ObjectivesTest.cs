using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Tdms.Tests
{
    [TestClass]
    public class ObjectivesTest
    {
        private static readonly string TEST_FILE_PATH = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "IntegrationTestFile.txt");

        private static ObjectiveService objectiveService;
        private static ObjectiveExternalDto objectiveDefect;
        private static BimElementExternalDto bimElement;
        private static ItemExternalDto item;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            // TODO: Find job and find issues and use their guids to test?
            objectiveService = new ObjectiveService();

            var objectiveTypeDefect = new ObjectiveTypeExternalDto()
            {
                Name = ObjectTypeID.DEFECT,
            };

            var objectiveTypeWork = new ObjectiveTypeExternalDto()
            {
                Name = ObjectTypeID.WORK,
            };

            bimElement = new BimElementExternalDto()
            {
                GlobalID = Guid.NewGuid().ToString(),
            };

            item = new ItemExternalDto()
            {
                Name = System.IO.Path.GetFileName(TEST_FILE_PATH),
                FullPath = TEST_FILE_PATH,
                ItemType = ItemTypeDto.File,
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
                DynamicFields = default,
                BimElements = new List<BimElementExternalDto>() { bimElement },
            };
        }

        [TestMethod]
        [DataRow("{C98B456D-AB4F-4D78-9748-4129DB8294D7}", DisplayName = "Get Defect")]
        [DataRow("{8CA2DD65-3D8B-43FC-A1DA-4356BE509461}", DisplayName = "Get Job")]
        public void GetObjective_ExistingObjectiveID_ReturnsObjectiveDto(string value)
        {
            var res = objectiveService.Get(value);
            Assert.IsNotNull(res);
            Assert.AreEqual(res.Items.Count, 1);
        }

        [TestMethod]
        public void AddObjective_NonExistingObjectiveDefectType_ReturnsObjectiveDto()
        {
            var objectiveDto = objectiveService.Add(objectiveDefect);
            Assert.IsNotNull(objectiveDto);
            Assert.IsNotNull(objectiveDto.ExternalID);
            Assert.AreEqual(objectiveDto.Items.Count, 1);
            Assert.AreEqual(objectiveDto.BimElements.Count, 1);

            // Remove added issue
            objectiveService.Remove(objectiveDto.ExternalID);
        }

        [TestMethod]
        public void UpdateObjective_ExistingObjectiveDefectType_ReturnsObjectiveDto()
        {
            var oldValues = objectiveService.Get("{C98B456D-AB4F-4D78-9748-4129DB8294D7}");

            var objective = objectiveService.Get("{C98B456D-AB4F-4D78-9748-4129DB8294D7}");
            objective.CreationDate = objective.CreationDate.AddDays(1);
            objective.DueDate = objective.DueDate.AddDays(1);
            objective.Description += " (Updated)";
            objective.Title += " (Updated)";
            objective.BimElements.Add(bimElement);

            var updatedObjective = objectiveService.Update(objective);
            Assert.AreEqual(objective.Description, updatedObjective.Description);
            Assert.AreEqual(objective.Title, updatedObjective.Title);
            Assert.AreEqual(objective.DueDate, updatedObjective.DueDate);
            Assert.AreEqual(objective.CreationDate, updatedObjective.CreationDate);

            Assert.IsNotNull(updatedObjective.BimElements);
            Assert.AreEqual(objective.BimElements.Count, updatedObjective.BimElements.Count);

            // Return previous values
            objectiveService.Update(oldValues);
        }

        [TestMethod]
        public void RemoveObjective_ExistingObjectiveDefectType_ReturnsTrue()
        {
            var objectiveDto = objectiveService.Add(objectiveDefect);
            var res = objectiveService.Remove(objectiveDto.ExternalID);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void GetListOfObjectives_ReturnsListOfObjectiveDto()
        {
            // Warning! Really slow.
            //var list = objectiveService.GetListOfObjectives();
            //Assert.IsNotNull(list);
        }
    }
}
