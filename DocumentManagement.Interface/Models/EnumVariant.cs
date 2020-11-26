namespace MRS.DocumentManagement.Interface.Models
{
    public struct EnumVariant
    {
        public EnumVariant(ID<EnumVariant> iD, string name)
        {
            ID = iD;
            Name = name;
        }

        public ID<EnumVariant> ID { get; }
        public string Name { get; }
    }
}
