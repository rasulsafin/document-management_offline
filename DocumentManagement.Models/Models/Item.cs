namespace DocumentManagement.Models
{
    public class Item: Entity
    {
        public TypeItemDm Type { get; set; }
        public string Path { get; set; }
    }
}
