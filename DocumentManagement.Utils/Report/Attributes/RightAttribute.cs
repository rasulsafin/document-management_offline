using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MRS.DocumentManagement.Utils.ReportCreator.Attributes
{
    internal class RightAttribute : IAttribute
    {
        public void Apply(OpenXmlElement element) => element.Append(new Justification() { Val = JustificationValues.Right });
    }
}
