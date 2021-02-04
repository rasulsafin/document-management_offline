using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Forge
{
    public class Issue: CloudItem<,>
    {
        private const string delimiter = "&,";

        [DataMember]
        public string type;
        [DataMember]
        public Attributes attributes;

        public List<File> attachments = new List<File>();

        [DataContract]
        public class Attributes
        {
            [DataMember(EmitDefaultValue = false)]
            public DateTime? created_at;
            [DataMember(EmitDefaultValue = false)]
            public DateTime? due_date;
            [DataMember(EmitDefaultValue = false)]
            public string title;
            [DataMember(EmitDefaultValue = false)]
            public string description;
            [DataMember(EmitDefaultValue = false)]
            public string ng_issue_type_id;
            [DataMember(EmitDefaultValue = false)]
            public string ng_issue_subtype_id;
            [DataMember(EmitDefaultValue = false)]
            public string status;
            [DataMember(EmitDefaultValue = false)]
            public string created_by;
            [DataMember(EmitDefaultValue = false)]
            public string location_description;
            [DataMember(EmitDefaultValue = false)]
            public string answer;

        }

        public async Task<Attachment> AttachFile(Project project, Attachment attachment)
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var response = await JsonData<CloudItem<,>>.PostSerializedData("/issues/v1/containers/{container_id}/attachments",
                new Dictionary<string, string> { ["container_id"] = project.issuesContainerId }, new CloudItem<,>(attachment, "attachments"));
            return null;
        }
    }
}
