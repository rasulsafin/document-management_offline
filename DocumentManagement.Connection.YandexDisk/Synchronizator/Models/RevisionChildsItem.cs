using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class RevisionChildsItem : Revision
    {
        public RevisionChildsItem(int id, ulong rev = 0)
            : base(id, rev)
        {
        }

        public List<Revision> Items { get; set; }

        public Revision GetItem(int id) => FindItem(id);

        private Revision FindItem(int id)
        {
            if (Items == null) Items = new List<Revision>();
            Revision item = Items.Find(x => x.ID == id);
            if (item == null)
            {
                item = new Revision(id);
                Items.Add(item);
            }

            return item;
        }
    }
}
