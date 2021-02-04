using System;

namespace MRS.DocumentManagement.Interface.Services
{
    public struct ProgressSync
    {
        /// <summary>total  - total number of elements found</summary>
        public int total;

        /// <summary>message  - short name of the synchronization stage</summary>
        public string message;

        /// <summary>current - synchronized items at the moment</summary>
        public int current;

        public Exception error;
    }
}
