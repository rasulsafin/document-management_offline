using DocumentFormat.OpenXml;

namespace MRS.DocumentManagement.Utils.ReportCreator.Attributes
{
    internal interface IAttribute
    {
        void Apply(OpenXmlElement element);
    }
}
