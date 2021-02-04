using System;

namespace MRS.DocumentManagement.Interface.Services
{
    public struct ProgressSync
    {
        /// <summary>total  - total number of elements found</summary>
        public int Total { get; set; }

        /// <summary>message  - short name of the synchronization stage</summary>
        public string Message { get; set; }

        /// <summary>current - synchronized items at the moment</summary>
        public int Current { get; set; }

        public Exception Error { get; set; }
    }
}
