using System.Collections.Generic;
using Newtonsoft.Json;

namespace MRS.Bim.DocumentManagement
{
    /// <summary>
    /// Info about connection
    /// </summary>
    public class ConnectionInfo
    {
        [JsonProperty]
        private readonly string imagePath;

        [JsonProperty]
        public readonly string error;

        [JsonProperty]
        public string Name { get; }
        [JsonProperty]
        public DMAccount Account { get; set; }
        [JsonProperty]
        public Dictionary<string, DMItem[]> Enums { get; set; }

        [JsonProperty]
        public int ID
        {
            get
            {
                if (id == 0)
                    id = Name.GetHashCode();
                return id;
            }
            set => id = value;
        }
        private static int id;

        public ConnectionInfo() { }
        public ConnectionInfo(int id, string name, Dictionary<string, DMItem[]> enums, string err) =>
            (ID, Name, Enums, error) = (id, name, enums, err);
    }
}
 