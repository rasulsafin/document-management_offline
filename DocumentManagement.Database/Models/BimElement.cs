﻿using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class BimElement
    {
        public int ID { get; set; }

        public string GlobalID { get; set; }

        public string ParentName { get; set; }

        public string ElementName { get; set; }

        public ICollection<BimElementObjective> Objectives { get; set; }
    }
}
