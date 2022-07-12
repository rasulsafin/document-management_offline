using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Elements
{
    internal class TableElement : AElement
    {
        private static Table tablePrototype;

        public static void RemoveTablePrototype()
        {
            tablePrototype.Remove();
            tablePrototype = null;
        }

        public override void Read(XElement node, OpenXmlElement element)
        {
            if (!(element is Body))
                return;

            if (tablePrototype == null)
            {
                tablePrototype = element.Elements<Table>().LastOrDefault();

                if (tablePrototype == null)
                {
                    tablePrototype = new Table();
                }
            }

            var table = tablePrototype.CloneNode(true);
            element.Append(table);

            foreach (var subnode in node.Elements())
                ReportCreator.Read(subnode, table);
        }
    }
}
