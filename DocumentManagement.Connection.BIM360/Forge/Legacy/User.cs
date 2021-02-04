using System.Runtime.Serialization;

namespace Forge
{
    public class User : CloudItem<,>
    {
        [DataMember]
        public Attributes attributes;

        [DataContract]
        public class Attributes
        {
            [DataMember]
            public string name;
            [DataMember]
            public Role role;
        }

        public enum Role
        {
            project_admin,
            project_user
        }
    }
}
