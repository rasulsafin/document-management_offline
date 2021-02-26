using System.Collections.Generic;

namespace MRS.DocumentManagement.Synchronizer
{
    public class RevisionCollection
    {
        public Dictionary<NameTypeRevision, List<Revision>> DataRev { get; set; } = new Dictionary<NameTypeRevision, List<Revision>>();

        public Revision GetRevision(NameTypeRevision table, int id)
        {
            var listRev = GetRevisions(table);
            Revision rev = listRev.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new Revision(id);
                listRev.Add(rev);
            }

            return rev;
        }

        public List<Revision> GetRevisions(NameTypeRevision table)
        {
            if (!DataRev.ContainsKey(table))
                DataRev.Add(table, new List<Revision>());
            return DataRev[table];
        }
    }
}
