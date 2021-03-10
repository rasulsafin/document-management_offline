using System;
using MRS.DocumentManagement.Database;

namespace MRS.DocumentManagement.Synchronization.Models
{
    public enum ObjectType
    {
        Local,
        Remote,
    }

    public class SynchronizingResult
    {
        public ISynchronizableBase Object { get; set; }

        public ObjectType ObjectType { get; set; }

        public Exception Exception { get; set; }
    }
}
