using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizer
{
    public class Revisions
    {
        public Revisions()
        {
            Projects = new Dictionary<int, ulong>(); ;
            Users = new Dictionary<int, ulong>(); ;
            Objectives = new Dictionary<int, Dictionary<int, ulong>>();
            ItemsProject = new Dictionary<int, Dictionary<int, ulong>>();
            ItemsObjective = new Dictionary<int, Dictionary<int, Dictionary<int, ulong>>>();
        }
        /// <summary>
        /// Project -> Revision
        /// </summary>
        public Dictionary<int, ulong> Projects { get; set; }
        /// <summary>
        /// Users -> Revision
        /// </summary>
        public Dictionary<int, ulong> Users { get; set; }
        /// <summary>
        /// Project -> Objective -> Revision
        /// </summary>
        public Dictionary<int, Dictionary<int, ulong>> Objectives { get; set; }
        /// <summary>
        /// Project -> Item -> Revision
        /// </summary>
        public Dictionary<int, Dictionary<int, ulong>> ItemsProject { get; set; }
        /// <summary>
        /// Project -> Objective -> Item -> Revision
        /// </summary>
        public Dictionary<int, Dictionary<int, Dictionary<int, ulong>>> ItemsObjective { get; set; }
        //public ulong Project { get; set; }
        //public ulong Project { get; set; }

    }
}
