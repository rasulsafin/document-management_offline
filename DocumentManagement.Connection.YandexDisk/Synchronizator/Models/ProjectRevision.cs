using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ProjectRevision : RevisionChildsItem
    {
        // public static implicit operator Revision(ProjectRevision project)
        // {
        //    return (Revision)project;
        // }
        public ProjectRevision(int id, ulong rev = 0) : base(id, rev) { }

        public List<RevisionChildsItem> Objectives { get; set; }

        public void UpdateObjective(int id)
        {
            RevisionChildsItem objective = FindObjetive(id);
            objective.Incerment();
            Incerment();
        }

        public RevisionChildsItem FindObjetive(int id)
        {
            if (Objectives == null) Objectives = new List<RevisionChildsItem>();
            RevisionChildsItem objective = Objectives.Find(x => x.ID == id);
            if (objective == null)
            {
                objective = new RevisionChildsItem(id);
                Objectives.Add(objective);
            }

            return objective;
        }

        //public void UpdateItem(int idObj, int id)
        //{
        //    RevisionChildsItem objective = FindObjetive(idObj);
        //    objective.UpdateItem(id);
        //    Incerment();
        //}
    }
}
