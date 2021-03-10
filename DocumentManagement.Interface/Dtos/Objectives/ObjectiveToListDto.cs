using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ObjectiveToListDto
    {
        public ID<ObjectiveDto> ID { get; set; }

        public string Title { get; set; }

        public ObjectiveStatus Status { get; set; }

        public ObjectiveTypeDto ObjectiveType { get; set; }

        public IEnumerable<BimElementDto> BimElements { get; set; }
    }
}