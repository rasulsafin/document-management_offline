using System.Collections.Generic;
using System.Linq;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizator
{
    public class RevisionCollection
    {
        public int Total { get; set; }

        public List<ProjectRevision> Projects { get; set; }
        public List<Revision> Users { get; set; }

        public void UpdateUser(int id)
        {
            if (Users == null) Users = new List<Revision>();
            var rev = Users.Find(x => x.ID == id);
            if (rev == null)
            {
                rev = new Revision(id);
                Users.Add(rev);
            }
            rev.Incerment();
        }

        public void UpdateProject(int id)
        {
            ProjectRevision rev = FindProject(id);
            rev.Incerment();

        }

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

        public void UpdateObjective(int idProj, int id)
        {
            ProjectRevision project = FindProject(idProj);            
            project.UpdateObjective(id);
        }

        public void UpdateItem(int idProj, int id)
        {
            ProjectRevision project = FindProject(idProj);
            project.UpdateItem(id);
        }

        public void UpdateItem(int idProj, int idObj, int id)
        {
            ProjectRevision project = FindProject(idProj);
            project.UpdateItem(idObj, id);
        }
    }
}
