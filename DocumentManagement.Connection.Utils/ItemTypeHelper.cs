using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Utils
{
    public static class ItemTypeHelper
    {
        // TODO: improve this.
        private static readonly IReadOnlyCollection<string> BIM_EXTENSIONS = new[] { ".ifc" };
        private static readonly IReadOnlyCollection<string> MEDIA_EXTENSIONS = new[] { ".jpg", ".mp4" };

        public static ItemTypeDto GetTypeByName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return BIM_EXTENSIONS.Any(s => string.Equals(s, extension, StringComparison.InvariantCultureIgnoreCase))
                ? ItemTypeDto.Bim
                : MEDIA_EXTENSIONS.Any(s => string.Equals(s, extension, StringComparison.InvariantCultureIgnoreCase))
                    ? ItemTypeDto.Media
                    : ItemTypeDto.File;
        }
    }
}
