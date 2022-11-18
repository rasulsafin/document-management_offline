using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly Lazy<string> XSLT_HEADER = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ROOT_FOLDER, "HeaderStyles.xslt"));
        private static readonly Lazy<string> TEMPLATE_DOCUMENT = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ROOT_FOLDER, "ReportTemplate.docx"));
        private static readonly Lazy<string> LOGO_BIG = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ROOT_FOLDER, "image1.jpg"));
        private static readonly Lazy<string> LOGO_SMALL = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ROOT_FOLDER, "image2.png"));

        private static readonly string ROOT = "Report";

        private static readonly string LOGO_IMAGE_ID = "rId1";

        internal static MainDocumentPart MainPart { get; set; }

        public void CreateReport(XDocument xml, XDocument footer, XDocument header, string outputDocument)
        {
            if (File.Exists(XSLT_FILE.Value) &&
                File.Exists(XSLT_FOOTER.Value) &&
                File.Exists(XSLT_HEADER.Value) &&
                File.Exists(TEMPLATE_DOCUMENT.Value) &&
                File.Exists(LOGO_BIG.Value) &&
                File.Exists(LOGO_SMALL.Value))
            {
                var bodyDocument = GetDocument(XSLT_FILE.Value, TEMPLATE_DOCUMENT.Value, xml.ToString(), outputDocument);
                var footerDocument = GetDocument(XSLT_FOOTER.Value, TEMPLATE_DOCUMENT.Value, footer.ToString(), outputDocument);
                var headerDocument = GetDocument(XSLT_HEADER.Value, TEMPLATE_DOCUMENT.Value, header.ToString(), outputDocument);
                var logoBigStream = new FileStream(LOGO_BIG.Value, FileMode.Open);
                var logoSmallStream = new FileStream(LOGO_SMALL.Value, FileMode.Open);

                CreateWithTemplate(bodyDocument, footerDocument, headerDocument, outputDocument, logoBigStream, logoSmallStream);
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

        private void CreateWithTemplate(
            XmlDocument bodyDocument,
            XmlDocument footerDocument,
            XmlDocument headerDocument,
            string outputDocument,
            FileStream logoBigStream,
            FileStream logoSmallStream)
        {
            using (WordprocessingDocument output = WordprocessingDocument.Open(outputDocument, true))
            {
                Body updatedBodyContent = new Body(bodyDocument.DocumentElement.InnerXml);
                //Footer footer = new Footer(footerDocument.DocumentElement.OuterXml);
                //Header header = new Header(headerDocument.DocumentElement.OuterXml);

                var mainPart = output.MainDocumentPart;

                mainPart.Document.Body = updatedBodyContent;

                //mainPart.DeleteParts(mainPart.FooterParts);
                //var footerPart = mainPart.AddNewPart<FooterPart>();
                //footerPart.Footer = footer;
                //var footerId = mainPart.GetIdOfPart(footerPart);

                //mainPart.DeleteParts(mainPart.HeaderParts);
                //var headerPart = mainPart.AddNewPart<HeaderPart>();
                //headerPart.Header = header;
                //var headerId = mainPart.GetIdOfPart(headerPart);

                //var sectPrsMainBody = mainPart.Document.Body.Elements<SectionProperties>();
                //foreach (var sectPr in sectPrsMainBody)
                //{
                //    sectPr.RemoveAllChildren<HeaderReference>();
                //    sectPr.PrependChild<HeaderReference>(new HeaderReference() { Id = headerId, Type = new EnumValue<HeaderFooterValues>(HeaderFooterValues.First) });

                //    sectPr.RemoveAllChildren<FooterReference>();
                //    sectPr.PrependChild<FooterReference>(new FooterReference() { Id = footerId, Type = new EnumValue<HeaderFooterValues>(HeaderFooterValues.Default) });
                //}

                //var bigLogoImagePart = headerPart.AddImagePart(ImagePartType.Jpeg, LOGO_IMAGE_ID);
                //bigLogoImagePart.FeedData(logoBigStream);

                //var smallLogoImagePart = footerPart.AddImagePart(ImagePartType.Png, LOGO_IMAGE_ID);
                //smallLogoImagePart.FeedData(logoSmallStream);

                logoBigStream.Close();
                logoSmallStream.Close();

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
