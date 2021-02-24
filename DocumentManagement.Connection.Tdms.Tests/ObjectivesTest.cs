using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Tdms.Helpers;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Tdms.Tests
{
    [TestClass]
    public class ObjectivesTest
    {
        private static ObjectiveService objectiveService;

        private static ObjectiveExternalDto objectiveDefect;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            objectiveService = new ObjectiveService();

            var objectiveTypeDefect = new ObjectiveTypeExternalDto()
            {
                Name = ObjectTypeID.DEFECT,
            };

            var objectiveTypeWork = new ObjectiveTypeExternalDto()
            {
                Name = ObjectTypeID.WORK,
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
                Items = default,
                DynamicFields = default,
                BimElements = default,
            };
        }

        [TestMethod]
        [DataRow("{C98B456D-AB4F-4D78-9748-4129DB8294D7}")] // Defect
        //[DataRow("{8CA2DD65-3D8B-43FC-A1DA-4356BE509461}")] // Job
        public void GetObjective_ExistingObjectiveID_ReturnsObjectiveDto(string value)
        {
            var res = objectiveService.Get(value);
            Assert.IsNotNull(res);
        }

        [TestMethod]
        public void AddObjective_NonExistingObjective_ReturnsObjectiveDto()
        {
            var objectiveDto = objectiveService.Add(objectiveDefect);
            Assert.IsNotNull(objectiveDto);
            Assert.IsNotNull(objectiveDto.ExternalID);

            // Remove added issue
            objectiveService.Remove(objectiveDto.ExternalID);
        }

        [TestMethod]
        public void UpdateObjective_ExistingObjective_ReturnsObjectiveDto()
        {
            var oldValues = objectiveService.Get("{C98B456D-AB4F-4D78-9748-4129DB8294D7}");

            var objective = objectiveService.Get("{C98B456D-AB4F-4D78-9748-4129DB8294D7}");
            objective.CreationDate = objective.CreationDate.AddDays(1);
            objective.DueDate = objective.DueDate.AddDays(1);
            objective.Description += " (Updated)";
            objective.Title += " (Updated)";

            var updatedObjective = objectiveService.Update(objective);
            Assert.AreEqual(objective.Description, updatedObjective.Description);
            Assert.AreEqual(objective.Title, updatedObjective.Title);
            Assert.AreEqual(objective.DueDate, updatedObjective.DueDate);
            Assert.AreEqual(objective.CreationDate, updatedObjective.CreationDate);

            // Return previous values
            objectiveService.Update(oldValues);
        }

        [TestMethod]
        public void RemoveObjective_ExistingObjective_ReturnsTrue()
        {
            var objectiveDto = objectiveService.Add(objectiveDefect);
            var res = objectiveService.Remove(objectiveDto.ExternalID);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void GetListOfObjectives_ReturnsListOfObjectiveDto()
        {

        }

    }
}
