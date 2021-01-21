using System;
using System.Collections.Generic;
using System.Linq;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class RevisionCollection
    {
        public int Total { get; set; }

        public List<ProjectRevision> Projects { get; set; }

        public List<Revision> Users { get; set; }

        public ProjectRevision GetProject(int id) => FindProject(id);

        public Revision GetUser(int id) => FindUser(id);

        public Revision GetObjective(int idProj, int id) => FindProject(idProj).FindObjetive(id);

        public Revision GetItem(int idProj, int id) => FindProject(idProj).FindItem(id);

        public Revision GetItem(int idProj, int idObj, int id) => FindProject(idProj).FindObjetive(idObj).FindItem(id);

        private ProjectRevision FindProject(int id)
        {
            if (Projects == null) Projects = new List<ProjectRevision>();
            ProjectRevision rev = Projects.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new ProjectRevision(id);
                Projects.Add(rev);
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
