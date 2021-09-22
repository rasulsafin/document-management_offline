using Brio.Docs.Database;
using System;

namespace Brio.Docs.Synchronization.Models
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
