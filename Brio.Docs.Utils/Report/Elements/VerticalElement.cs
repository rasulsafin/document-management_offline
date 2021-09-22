using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Elements
{
    internal class VerticalElement : AElement
    {
        public override void Read(XElement node, OpenXmlElement element)
        {
            foreach (var subnode in node.Elements())
                ReportCreator.Read(subnode, element);

            element.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }
    }
}
