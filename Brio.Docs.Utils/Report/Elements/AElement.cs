using Brio.Docs.Utils.ReportCreator.Attributes;
using System.Xml.Linq;
using DocumentFormat.OpenXml;

namespace Brio.Docs.Utils.ReportCreator.Elements
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
