using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Attributes
{
    internal class HeadingAttribute : IAttribute
    {
        private static readonly string HEADING_SIZE_DOUBLE = "40";

        public void Apply(OpenXmlElement element) => element.Append(new FontSize() { Val = HEADING_SIZE_DOUBLE });
    }
}
