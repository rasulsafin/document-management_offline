using System;
using System.Collections.Generic;
using System.Linq;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class RevisionCollection
    {
        public int Total { get; set; }

        public List<RevisionChildsItem> Projects { get; set; }

        public List<RevisionChildsItem> Objectives { get; set; }

        public List<Revision> Users { get; set; }

        public RevisionChildsItem GetProject(int id) => FindProject(id);

        public Revision GetUser(int id) => FindUser(id);

        public RevisionChildsItem GetObjective(int id) => FindObjective(id);

        //public Revision GetItem(int idProj, int id) => FindProject(idProj).FindItem(id);

        //public Revision GetItem(int idProj, int idObj, int id) => FindProject(idProj).FindObjetive(idObj).FindItem(id);

        private RevisionChildsItem FindProject(int id)
        {
            if (Projects == null) Projects = new List<RevisionChildsItem>();
            RevisionChildsItem rev = Projects.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new RevisionChildsItem(id);
                Projects.Add(rev);
            }

            return rev;
        }

        private RevisionChildsItem FindObjective(int id)
        {
            if (Objectives == null) Objectives = new List<RevisionChildsItem>();
            RevisionChildsItem rev = Objectives.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new RevisionChildsItem(id);
                Objectives.Add(rev);
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
