using System;
using System.Collections.Generic;
using System.Linq;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizer
{
    public class Revisions
    {
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

    public class ProjectRevision : ObjectiveRevision
    {
        public ProjectRevision(int id, ulong rev = 0) : base(id, rev){}
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

    public class ObjectiveRevision : Revision
    {
        public ObjectiveRevision(int id, ulong rev = 0) : base(id, rev){}

        public List<Revision> Items { get; set; }

        public Revision FindItem(int id)
        {
            if (Items == null) Items = new List<Revision>();
            Revision objective = Items.Find(x => x.ID == id);
            if (objective == null)
            {
                objective = new Revision(id);
                Items.Add(objective);
            }
            return objective;
        }

        public void UpdateItem(int id)
        {
            Revision item = FindItem(id);
            item.Incerment();
            Incerment();
        }
    }

    public class Revision : IComparable<Revision>
    {
        public void Incerment() => Rev++;

        public override string ToString() => $"{ID}=>{Rev}";

        public int CompareTo(Revision other) => Rev.CompareTo(other.Rev);

        public static bool operator >(Revision rev1, Revision rev2) => rev1.Rev > rev2.Rev;
        public static bool operator <(Revision rev1, Revision rev2) => rev1.Rev < rev2.Rev;
        //public static bool operator ==(Revision rev1, Revision rev2) => rev1.Equals(rev2);
        //public static bool operator !=(Revision rev1, Revision rev2) => !rev1.Equals(rev2);

        public Revision(int id, ulong rev = 0)
        {
            ID = id;
            Rev = rev;
        }

        public int ID { get; set; }
        public ulong Rev { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj is Revision rev)
            {
                return ID == rev.ID && Rev == rev.Rev;
            }
            return false;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator <=(Revision left, Revision right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Revision left, Revision right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
