using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MRS.DocumentManagement.Utils.ReportCreator.Elements
{
    internal class TableElement : AElement
    {
        public override void Read(XElement node, OpenXmlElement element)
        {
            if (!(element is Body))
                return;

            Table table =
               element.Elements<Table>().Last();

            ///TODO: if table == null create new one

            foreach (var subnode in node.Elements())
                ReportCreator.Read(subnode, table);
        }
    }
}
