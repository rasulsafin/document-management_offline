using System;
using System.Collections.Generic;
using System.Linq;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class RevisionCollection
    {
        public int Total { get; set; }

        public List<Revision> Projects { get; set; }

        public List<Revision> Objectives { get; set; }

        public List<Revision> Users { get; set; }

        public List<Revision> Items { get; set; }

        public Revision GetProject(int id) => FindProject(id);

        public Revision GetUser(int id) => FindUser(id);

        public Revision GetObjective(int id) => FindObjective(id);

        //public Revision GetItem(int idProj, int id) => FindProject(idProj).FindItem(id);

        //public Revision GetItem(int idProj, int idObj, int id) => FindProject(idProj).FindObjetive(idObj).FindItem(id);

        private Revision FindProject(int id)
        {
            if (Projects == null) Projects = new List<Revision>();
            Revision rev = Projects.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new Revision(id);
                Projects.Add(rev);
            }

            return rev;
        }

        private Revision FindObjective(int id)
        {
            if (Objectives == null) Objectives = new List<Revision>();
            Revision rev = Objectives.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new Revision(id);
                Objectives.Add(rev);
            }

            return rev;
        }

        public Revision GetItem(int id)
        {
            if (Items == null) Items = new List<Revision>();
            var rev = Items.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new Revision(id);
                Items.Add(rev);
            }

            return rev;
        }

        private Revision FindUser(int id)
        {
            if (Users == null) Users = new List<Revision>();
            var rev = Users.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new Revision(id);
                Users.Add(rev);
            }

            return rev;
        }
    }
}
