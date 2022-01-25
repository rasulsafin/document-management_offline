using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Attributes
{
    internal class RegularAttribute : IAttribute
    {
        private static readonly string TEXT_SIZE_DOUBLE = "22";

        public void Apply(OpenXmlElement element) => element.Append(new FontSize() { Val = TEXT_SIZE_DOUBLE });
    }
}
