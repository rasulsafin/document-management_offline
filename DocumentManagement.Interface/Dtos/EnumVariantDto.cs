namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct EnumVariantDto
    {
        public ID<EnumVariantDto> ID { get; }
        public string Name { get; }

        public EnumVariantDto(ID<EnumVariantDto> iD, string name)
        {
            ID = iD;
            Name = name;
        }
    }
}
