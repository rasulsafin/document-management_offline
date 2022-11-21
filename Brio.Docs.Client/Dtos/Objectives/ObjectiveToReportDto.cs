using System;
using System.Collections.Generic;
using Brio.Docs.Common;

namespace Brio.Docs.Client.Dtos
{
    public struct ObjectiveToReportDto
    {
        public string ID { get; set; }

        public string Author { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IEnumerable<BimElementDto> BimElements { get; set; }

        public IEnumerable<ItemDto> Items { get; set; }

        public LocationDto Location { get; set; }

        public ObjectiveStatus Status { get; set; }
    }
}
