using MRS.Bim.DocumentManagement.Tdms.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class Job : DMItemExtra, IGuidLinkable
    {
        [JsonProperty]
        public Status Status { get; set; }

        [JsonConverter(typeof(VolumeJsonConverter))]
        public dynamic Volume { get; set; }

        [JsonConverter(typeof(OperationJsonConverter))]
        public List<dynamic> Operations { get; set; }
        [JsonProperty]
        public List<DMFile> Blueprints { get; set; }

        /// <summary>
        /// Gameobjects' metadata's guids' links
        /// </summary>
        public List<string> Links { get; set; }

    }
}
