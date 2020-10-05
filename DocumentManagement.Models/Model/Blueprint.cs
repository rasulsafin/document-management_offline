using System;
using System.Collections.Generic;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class Blueprint : DMFile, IGuidLinkable
    {
        /// <summary>
        /// Gameobjects' metadata's guids' links
        /// </summary>
        public List<string> Links { get; set; }
    }
}