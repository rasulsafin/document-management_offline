using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MRS.DocumentManagement.Utils.ReportCreator.Elements
{
    internal class HorizontalElement : AElement
    {
        public override void Read(XElement node, OpenXmlElement element)
        {
            Paragraph paragraph = new Paragraph();
            ParagraphProperties paragraphProperties = new ParagraphProperties();

            SetAttributes(node, paragraphProperties);

            paragraph.Append(paragraphProperties);
            element.Append(paragraph);

            foreach (var subnode in node.Elements())
                ReportCreator.Read(subnode, paragraph);
        }
    }
}
