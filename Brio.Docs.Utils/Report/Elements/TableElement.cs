using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Elements
{
    internal class TableElement : AElement
    {
        private static Table table;

        public static void RemoveTable()
        {
            table.Remove();
            table = null;
        }

        public override void Read(XElement node, OpenXmlElement element)
        {
            if (!(element is Body))
                return;

            if (table == null)
            {
                table = element.Elements<Table>().LastOrDefault();

                if (table == null)
                {
                    table = new Table();
                }
            }

            var newtable = table.CloneNode(true);
            element.Append(newtable);

            foreach (var subnode in node.Elements())
                ReportCreator.Read(subnode, newtable);
        }
    }
}
