using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ProjectRevision : RevisionChildrenItem
    {
        public ProjectRevision(int id, ulong rev = 0)
            : base(id, rev)
        {
        }

        public List<RevisionChildrenItem> Objectives { get; set; }

        public void UpdateObjective(int id)
        {
            RevisionChildrenItem objective = FindObjetive(id);
            objective.Incerment();
            Incerment();
        }

        public RevisionChildrenItem FindObjetive(int id)
        {
            if (Objectives == null) Objectives = new List<RevisionChildrenItem>();
            RevisionChildrenItem objective = Objectives.Find(x => x.ID == id);
            if (objective == null)
            {
                objective = new RevisionChildrenItem(id);
                Objectives.Add(objective);
            }

            return objective;
        }
    }
}
