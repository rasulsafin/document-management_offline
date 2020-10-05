using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MRS.Bim.DocumentManagement
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DMFileType
    {
        All,
        Ifc,
        Document,
        Media,
        Other,
    }
}
