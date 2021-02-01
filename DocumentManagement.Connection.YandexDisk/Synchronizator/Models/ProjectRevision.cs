using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ProjectRevision : RevisionChildsItem
    {
        public ProjectRevision(int id, ulong rev = 0)
            : base(id, rev)
        {
        }

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
    }
}
