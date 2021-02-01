using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MRS.DocumentManagement.Utils.ReportCreator.Elements
{
    internal class CellElement : AElement
    {
        public override void Read(XElement node, OpenXmlElement element)
        {
            TableCell cell = new TableCell();
            element.Append(cell);

            foreach (var subnode in node.Elements())
                ReportCreator.Read(subnode, cell);
        }
    }
}
