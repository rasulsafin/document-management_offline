using System.Collections.Generic;

namespace Brio.Docs.Reports.Models
{
    public class ReportModel
    {
        public ReportDetails ReportInfo { get; set; }

        public List<ObjectiveDetails> Tasks { get; set; }
    }
}
