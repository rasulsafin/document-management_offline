using System.Collections.Generic;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class RevisionCollection
    {
        public Dictionary<TableRevision, List<Revision>> DataRev { get; set; } = new Dictionary<TableRevision, List<Revision>>();

        public Revision GetRevision(TableRevision table, int id)
        {
            var listRev = GetRevisions(table);
            Revision rev = GetRevision(listRev, id);

            return rev;
        }

        public List<Revision> GetRevisions(TableRevision table)
        {
            if (!DataRev.ContainsKey(table))
            {
                DataRev.Add(table, new List<Revision>());
            }

            return DataRev[table];
        }

        private static Revision GetRevision(List<Revision> listRev, int id)
        {
            var rev = listRev.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new Revision(id);
                listRev.Add(rev);
            }

            return rev;
        }
    }
}
