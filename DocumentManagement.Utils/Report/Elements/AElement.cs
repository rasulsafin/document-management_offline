using System.Xml.Linq;
using DocumentFormat.OpenXml;
using MRS.DocumentManagement.Utils.ReportCreator.Attributes;

namespace MRS.DocumentManagement.Utils.ReportCreator.Elements
{
    internal abstract class AElement : IElement
    {
        public abstract void Read(XElement node, OpenXmlElement element);

        public void SetAttributes(XElement node, OpenXmlElement properties)
        {
            if (node.HasAttributes)
            {
                foreach (var attr in node.Attributes())
                {
                    Style.GetAttribute(attr)?.Apply(properties);
                }
            }
        }
    }
}
