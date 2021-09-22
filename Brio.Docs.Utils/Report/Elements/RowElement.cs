using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Elements
{
    internal class RowElement : AElement
    {
        public override void Read(XElement node, OpenXmlElement element)
        {
            TableRow row = new TableRow();
            element.Append(row);

            foreach (var subnode in node.Elements())
                ReportCreator.Read(subnode, row);
        }
    }
}
