using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Forge.Legacy
{
    public class IssueType : CloudItem<,>
    {
        [DataMember]
        public string title;

        [DataMember]
        public List<IssueSubtype> subtypes;

        public class IssueSubtype : CloudItem<,>
        {
            [DataMember]
            public string issueTypeId;
            [DataMember]
            public string title;
        }
    }
}