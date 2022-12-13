using System;
using System.IO;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Integration.Dtos
{
    public class ItemExternalDto
    {
        public string ExternalID { get; set; }

        public string FileName => Path.GetFileName(RelativePath);

        public string FullPath => Path.Combine(ProjectDirectory, RelativePath);

        public ItemType ItemType { get; set; }

        public string ProjectDirectory { get; set; }

        public string RelativePath { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
