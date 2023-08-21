using System.Collections.Generic;
using Brio.Docs.Client.Converters;
using Newtonsoft.Json;

namespace Brio.Docs.Client.Dtos
{
    public struct ProjectToListDto
    {
        public ID<ProjectDto> ID { get; set; }

        public string Title { get; set; }

        public IEnumerable<ItemDto> Items { get; set; }
    }
}
