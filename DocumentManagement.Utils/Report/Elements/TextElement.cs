using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MRS.DocumentManagement.Utils.ReportCreator.Elements
{
    internal class TextElement : AElement
    {
        private static readonly string TEXT_SIZE_DOUBLE = "22";

        public override void Read(XElement node, OpenXmlElement element)
        {
            if (!(element is Paragraph))
                return;

            Run run = new Run();
            RunProperties runProperties = new RunProperties();

            var fontSize = new FontSize() { Val = TEXT_SIZE_DOUBLE };
            runProperties.Append(fontSize);

            SetAttributes(node, runProperties);

            run.Append(runProperties);
            run.Append(new Text()
            {
                Text = node.Value,
                Space = SpaceProcessingModeValues.Preserve,
            });

            element.Append(run);
        }
    }
}
