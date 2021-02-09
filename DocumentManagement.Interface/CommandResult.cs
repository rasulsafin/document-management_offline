using System;
using System.Collections.Generic;
using System.Text;

namespace MRS.DocumentManagement.Interface
{
    public class CommandResult
    {
        public bool IsSuccessful { get; set; }

        public string Message { get; set; }
    }
}
