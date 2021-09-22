using Brio.Docs.Connection.Bim360.Forge.Models.DataManagement;
using System.Runtime.Serialization;

namespace Brio.Docs.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class RootCause : Object<RootCause.RootCauseAttributes, object>
    {
        public override string Type
        {
            get => Constants.ROOT_CAUSE_TYPE;
            set { }
        }

        [DataContract]
        public class RootCauseAttributes : AAttributes
        {
            [DataMember(Name = "key")]
            public string Key { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }
        }
    }
}
