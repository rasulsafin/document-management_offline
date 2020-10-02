using System.Collections.Generic;

namespace MRS.Bim.DocumentManagement
{
    public interface IGuidLinkable
    {
        List<string> Links { get; set; }
    }
}