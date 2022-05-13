using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Brio.Docs.Utils.ReportCreator.Elements;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Brio.Docs.Utils.ReportCreator
{
    public class ReportCreator
    {
        private static readonly Dictionary<string, IElement> ELEMENTS = new Dictionary<string, IElement>()
        {
            { "VerticalElement", new VerticalElement() },
            { "HorizontalElement", new HorizontalElement() },
            { "HeadingElement", new HeadingElement() },
            { "Text", new TextElement() },
            { "Image", new ImageElement() },
            { "Cell", new CellElement() },
            { "Row", new RowElement() },
            { "Table", new TableElement() },
        };

        private static readonly string ROOT_FOLDER = @"Report\Resources";
        private static readonly Lazy<string> XSLT_FILE = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ROOT_FOLDER, "ReportStyles.xslt"));
        private static readonly Lazy<string> XSLT_FOOTER = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ROOT_FOLDER, "FooterStyles.xslt"));
        private static readonly Lazy<string> TEMPLATE_DOCUMENT = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ROOT_FOLDER, "ReportTemplate.docx"));

        private static readonly string ROOT = "Report";

        internal static MainDocumentPart MainPart { get; set; }

        public void CreateReport(XDocument xml, XDocument footer, string outputDocument)
        {
            if (File.Exists(XSLT_FILE.Value) && File.Exists(TEMPLATE_DOCUMENT.Value))
            {
                var bodyDocument = GetDocument(XSLT_FILE.Value, TEMPLATE_DOCUMENT.Value, xml.ToString(), outputDocument);
                var footerDocument = GetDocument(XSLT_FOOTER.Value, TEMPLATE_DOCUMENT.Value, footer.ToString(), outputDocument);

                CreateWithTemplate(bodyDocument, footerDocument, outputDocument);
                Create(xml, outputDocument);
            }
        }

        internal static void Read(XElement node, OpenXmlElement element)
        {
            if (ELEMENTS.TryGetValue(node.Name.ToString(), out IElement value))
            {
                value.Read(node, element);
            }
            else
            {
                foreach (var subnode in node.Elements())
                    Read(subnode, element);
            }
        }

        private void Create(XDocument xml, string fileName)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(fileName, true))
            {
                MainPart = doc.MainDocumentPart;

                Body body = MainPart.Document.Body;
                Read(xml.Element(ROOT), body);

                TableElement.RemoveTablePrototype();
            }
        }

        private void CreateWithTemplate(XmlDocument bodyDocument, XmlDocument footerDocument, string outputDocument)
        {
            using (WordprocessingDocument output = WordprocessingDocument.Open(outputDocument, true))
            {
                Body updatedBodyContent = new Body(bodyDocument.DocumentElement.InnerXml);
                Footer footer = new Footer(footerDocument.DocumentElement.OuterXml);

                var mainPart = output.MainDocumentPart;

                mainPart.Document.Body = updatedBodyContent;

                mainPart.DeleteParts(mainPart.FooterParts);
                var footerPart = mainPart.AddNewPart<FooterPart>();
                footerPart.Footer = footer;
                var footerId = mainPart.GetIdOfPart(footerPart);

                var sectPrs = mainPart.Document.Body.Elements<SectionProperties>();
                foreach (var sectPr in sectPrs)
                {
                    sectPr.RemoveAllChildren<FooterReference>();
                    sectPr.PrependChild<FooterReference>(new FooterReference() { Id = footerId });
                }

                mainPart.Document.Save();
            }
        }

        private XmlDocument GetDocument(string xsltFile, string templateDocument, string xmlData, string outputDocument)
        {
            using (StringWriter stringWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
            using (StringReader stringReader = new StringReader(xmlData))
            {
                using (XmlReader xrt = XmlReader.Create(stringReader))
                {
                    XsltSettings sets = new XsltSettings(true, true);
                    var resolver = new XmlUrlResolver();

                    XslCompiledTransform transform = new XslCompiledTransform(true);
                    transform.Load(xsltFile, sets, resolver);
                    transform.Transform(xrt, xmlWriter);

                    XmlDocument newWordContent = new XmlDocument();
                    newWordContent.LoadXml(stringWriter.ToString());

                    File.Copy(templateDocument, outputDocument, true);

                    return newWordContent;
                }
            }
        }
    }
}
