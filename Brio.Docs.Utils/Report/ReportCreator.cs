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
using DocumentFormat.OpenXml.Validation;
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
        private static readonly Lazy<string> TEMPLATE_DOCUMENT = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ROOT_FOLDER, "ReportTemplate.docx"));

        private static readonly string ROOT = "Report";

        internal static MainDocumentPart MainPart { get; set; }

        public void CreateReport(XDocument xml, string outputDocument)
        {
            if (File.Exists(XSLT_FILE.Value) &&
                File.Exists(TEMPLATE_DOCUMENT.Value))
            {
                var bodyDocument = GetDocument(XSLT_FILE.Value, TEMPLATE_DOCUMENT.Value, xml.ToString(), outputDocument);

                CreateWithTemplate(bodyDocument, outputDocument);
                Create(xml, outputDocument);

                ValidateWordDocument(outputDocument);
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

        private static void ValidateWordDocument(string filepath)
        {
            using (WordprocessingDocument wordprocessingDocument =
            WordprocessingDocument.Open(filepath, true))
            {
                try
                {
                    OpenXmlValidator validator = new OpenXmlValidator();
                    int count = 0;
                    foreach (ValidationErrorInfo error in
                        validator.Validate(wordprocessingDocument))
                    {
                        count++;
                        System.Diagnostics.Debug.WriteLine("Error " + count);
                        System.Diagnostics.Debug.WriteLine("Description: " + error.Description);
                        System.Diagnostics.Debug.WriteLine("ErrorType: " + error.ErrorType);
                        System.Diagnostics.Debug.WriteLine("Node: " + error.Node);
                        System.Diagnostics.Debug.WriteLine("Path: " + error.Path.XPath);
                        System.Diagnostics.Debug.WriteLine("Part: " + error.Part.Uri);
                        System.Diagnostics.Debug.WriteLine("-------------------------------------------");
                    }

                    System.Diagnostics.Debug.WriteLine($"count={count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                wordprocessingDocument.Close();
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
            string outputDocument)
        {
            using (WordprocessingDocument output = WordprocessingDocument.Open(outputDocument, true))
            {
                Body updatedBodyContent = new Body(bodyDocument.DocumentElement.InnerXml);

                var mainPart = output.MainDocumentPart;

                mainPart.Document.Body = updatedBodyContent;

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
