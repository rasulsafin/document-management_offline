using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator.Elements
{
    internal class TextElement : AElement
    {
        public override void Read(XElement node, OpenXmlElement element)
        {
            if (!(element is Paragraph))
                return;

            Run run = new Run();
            RunProperties runProperties = new RunProperties();

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
