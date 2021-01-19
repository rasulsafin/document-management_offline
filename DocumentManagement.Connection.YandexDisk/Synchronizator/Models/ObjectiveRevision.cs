using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ObjectiveRevision : Revision
    {
        public ObjectiveRevision(int id, ulong rev = 0) : base(id, rev) { }

        public List<Revision> Items { get; set; }

        public Revision FindItem(int id)
        {
            if (Items == null) Items = new List<Revision>();
            Revision objective = Items.Find(x => x.ID == id);
            if (objective == null)
            {
                objective = new Revision(id);
                Items.Add(objective);
            }

            return objective;
        }

        public void UpdateItem(int id)
        {
            Revision item = FindItem(id);
            item.Incerment();
            Incerment();
        }
    }
}
