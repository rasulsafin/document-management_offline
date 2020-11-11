namespace DocumentManagement.Interface.Models
{
    public struct ID<T>
    {
        private readonly int id;

        public static explicit operator int(ID<T> ident) => ident.id;
        public static explicit operator ID<T>(int id) => new ID<T>(id);

        public ID(int id) => this.id = id;
    }
}
