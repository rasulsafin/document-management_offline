using DocumentFormat.OpenXml;

namespace Brio.Docs.Utils.ReportCreator.Attributes
{
    internal interface IAttribute
    {
        void Apply(OpenXmlElement element);
    }
}
