using System;
using System.Collections.Generic;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.UnitTests.Dummy
{
    internal static class DummyDtos
    {
        public static BimElementExternalDto BimElement
            => new ()
            {
                GlobalID = DummyStrings.BIM_ELEMENT_GLOBAL_ID,
                ParentName = "Model.ifc",
                ElementName = "Bim Element #1",
            };

        public static ItemExternalDto Item => new ()
        {
            ExternalID = DummyStrings.ITEM_ID,
            FileName = "Dummy file",
            FullPath = "C:\\Dummy Folder\\Dummy file.txt",
            ItemType = ItemType.File,
            UpdatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(-1)),
        };

        public static LocationExternalDto Location
            => new ()
            {
                Location = (0, 0, 0),
                CameraPosition = (0, 0, 0),
                Guid = DummyStrings.BIM_ELEMENT_GLOBAL_ID,
                Item = null,
            };

        public static ObjectiveExternalDto Objective
            => new ()
            {
                ExternalID = DummyStrings.ISSUE_ID,
                ProjectExternalID = DummyStrings.PROJECT_ID,
                ParentObjectiveExternalID = null,
                AuthorExternalID = DummyStrings.USER_ID,
                CreationDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
                DueDate = DateTime.UtcNow.AddDays(2),
                Title = "Dummy objective",
                Description = "Dummy objective description",
                Location = null,
                Status = ObjectiveStatus.Undefined,
                Items = new List<ItemExternalDto>(),
                DynamicFields = new List<DynamicFieldExternalDto>(),
                BimElements = new List<BimElementExternalDto>(),
                UpdatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(-1)),
            };
    }
}
