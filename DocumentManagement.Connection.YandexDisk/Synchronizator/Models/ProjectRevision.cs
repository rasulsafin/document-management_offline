using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ProjectRevision : ObjectiveRevision
    {
        // public static implicit operator Revision(ProjectRevision project)
        // {
        //    return (Revision)project;
        // }

        public ProjectRevision(int id, ulong rev = 0) : base(id, rev) { }
        public List<ObjectiveRevision> Objectives { get; set; }


        public void UpdateObjective(int id)
        {
            ObjectiveRevision objective = FindObjetive(id);
            objective.Incerment();
            Incerment();
        }

        public ObjectiveRevision FindObjetive(int id)
        {
            if (Objectives == null) Objectives = new List<ObjectiveRevision>();
            ObjectiveRevision objective = Objectives.Find(x => x.ID == id);
            if (objective == null)
            {
                objective = new ObjectiveRevision(id);
                Objectives.Add(objective);
            }

            return objective;
        }

        public void UpdateItem(int idObj, int id)
        {
            ObjectiveRevision objective = FindObjetive(idObj);
            objective.UpdateItem(id);
            Incerment();
        }
    }
}
