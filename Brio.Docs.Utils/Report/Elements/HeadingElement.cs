using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Elements
{
    internal class HeadingElement : AElement
    {
        public override void Read(XElement node, OpenXmlElement element)
        {
            var pageBreak = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
            element.Append(pageBreak);

            Paragraph paragraph = new Paragraph();
            ParagraphProperties paragraphProperties = new ParagraphProperties()
            {
                ParagraphStyleId = new ParagraphStyleId() { Val = "1" },
                Justification = new Justification() { Val = JustificationValues.Center },
            };

            SetAttributes(node, paragraphProperties);

            paragraph.Append(paragraphProperties);
            element.Append(paragraph);

            foreach (var subnode in node.Elements())
                ReportCreator.Read(subnode, paragraph);
        }
    }
}
